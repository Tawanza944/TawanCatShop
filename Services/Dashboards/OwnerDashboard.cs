using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Options;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

public sealed record OwnerAdminAccount(int Id, string Name, string Username);

public sealed record TopProduct(string Name, int Quantity, decimal Revenue);

public sealed record DailySales(DateOnly Day, int Orders, decimal Revenue);

public sealed record OwnerOverviewModel(ChartData Revenue, ChartData TopProducts);

public sealed record OwnerSalesModel(ChartData Revenue, IReadOnlyList<DailySales> Days, int RangeDays);

public sealed record OwnerRankingModel(ChartData Chart, IReadOnlyList<TopProduct> Products);

public sealed record OwnerFinanceModel(int DeliveredOrders, decimal Total, decimal Commission, decimal Transferred, decimal AwaitingTransfer, DateOnly From, DateOnly To, DateOnly Today);

public sealed class OwnerDashboard : DashboardBase
{
    public OwnerDashboard(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock)
        : base(db, commerce, currentUser, clock)
    {
    }

    public override string Role => Roles.Owner;

    public override IReadOnlyList<DashboardMenuItem> Menu { get; } =
    [
        new("overview", "ภาพรวม", "bi-grid"),
        new("sales", "ยอดขายรายวัน", "bi-graph-up"),
        new("finance", "ภาพรวมการเงิน", "bi-wallet2"),
        new("ranking", "สินค้าขายดี", "bi-bar-chart"),
        new("admin-access", "สิทธิ์ Admin", "bi-shield-lock")
    ];

    public override async Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken) => request.Section switch
    {
        "overview" => await OverviewAsync(cancellationToken),
        "sales" => await SalesAsync(request.Filter, cancellationToken),
        "finance" => await FinanceAsync(request, cancellationToken),
        "ranking" => await RankingAsync(cancellationToken),
        "admin-access" => await AdminAccessAsync(request.Page, cancellationToken),
        _ => null
    };

    private async Task<DashboardPage> AdminAccessAsync(int page, CancellationToken cancellationToken)
    {
        var admins = from user in Db.Users.AsNoTracking()
                     join staff in Db.Staff on user.Id equals staff.UserId
                     join role in Db.Roles on staff.RoleId equals role.Id
                     where role.Name == Roles.Admin
                     orderby user.Username, user.Id
                     select new OwnerAdminAccount(user.Id, user.Name, user.Username);

        return Page("admin-access", "สิทธิ์ Admin", string.Empty, await PageAsync(admins, page, cancellationToken));
    }

    private async Task<DashboardPage> OverviewAsync(CancellationToken cancellationToken)
    {
        var now = Clock.GetUtcNow().UtcDateTime;
        var current = await PeriodAsync(now.AddDays(-30), now, cancellationToken);
        var previous = await PeriodAsync(now.AddDays(-60), now.AddDays(-30), cancellationToken);
        var newCustomers = await Db.Users.CountAsync(u => u.CreatedAt >= now.AddDays(-30), cancellationToken);
        var newCustomersBefore = await Db.Users.CountAsync(u => u.CreatedAt >= now.AddDays(-60) && u.CreatedAt < now.AddDays(-30), cancellationToken);
        var settlement = await SettlementSummaryAsync(DeliveredBetween(Today.AddDays(-29), Today), cancellationToken);

        var revenue = await RevenueSeriesAsync(30, "ยอดขาย", cancellationToken) with { Kind = "line" };
        var top = await TopProductsAsync(5, cancellationToken);

        return Page("overview", "ภาพรวม", "_OwnerOverview",
            new OwnerOverviewModel(revenue, new ChartData("bar", top.Select(t => Shorten(t.Name)).ToList(), top.Select(t => t.Revenue).ToList(), "รายได้", "baht")),
            greeting: new GreetingBanner($"{Greeting()} คุณ{CurrentUser.DisplayName}",
                $"30 วันที่ผ่านมาร้านขายได้ {Baht(current.Revenue)} จาก {current.Orders} ออเดอร์",
                [new QuickAction("ดูยอดขายรายวัน", "sales", true), new QuickAction("ภาพรวมการเงิน", "finance"), new QuickAction("สินค้าขายดี", "ranking")]),
            metrics:
            [
                new MetricTile("รายได้ 30 วัน", Baht(current.Revenue), "bi-cash-stack", Percent(current.Revenue, previous.Revenue), current.Revenue >= previous.Revenue),
                new MetricTile("จำนวนออเดอร์", current.Orders.ToString("N0"), "bi-receipt", Percent(current.Orders, previous.Orders), current.Orders >= previous.Orders),
                new MetricTile("โอนให้ร้านแล้ว จากออเดอร์ 30 วัน", Baht(settlement.NetTransferred), "bi-bar-chart",
                    settlement.AwaitingCount > 0 ? $"รอบัญชีหักคอมอีก {settlement.AwaitingCount} ออเดอร์" : "หักคอมครบทุกออเดอร์แล้ว"),
                new MetricTile("ลูกค้าใหม่", newCustomers.ToString("N0"), "bi-people", Percent(newCustomers, newCustomersBefore), newCustomers >= newCustomersBefore)
            ]);
    }

    private async Task<DashboardPage> SalesAsync(string? filter, CancellationToken cancellationToken)
    {
        var days = filter switch { "7" => 7, "90" => 90, _ => 30 };
        var since = StartOfDayUtc(Today.AddDays(-(days - 1)));
        var orders = await Db.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= since && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.CreatedAt, o.TotalAmount }).ToListAsync(cancellationToken);
        var rows = orders.GroupBy(o => LocalDay(o.CreatedAt))
            .Select(g => new DailySales(g.Key, g.Count(), g.Sum(o => o.TotalAmount)))
            .OrderByDescending(d => d.Day).ToList();

        var chart = await RevenueSeriesAsync(days, "ยอดขาย", cancellationToken) with { Kind = "line" };
        return Page("sales", "ยอดขายรายวัน", string.Empty, new OwnerSalesModel(chart, rows, days), $"ย้อนหลัง {days} วัน ไม่นับออเดอร์ที่ยกเลิก");
    }

    /// <summary>
    /// สรุปรายได้ค่าคอมและต้นทุนของแพลตฟอร์มจากรายการบัญชี
    /// ค่าเริ่มต้นคือช่วงเดือนปัจจุบัน และแสดงรายการที่ยังรอฝ่ายบัญชีดำเนินการ
    /// </summary>
    private async Task<DashboardPage> FinanceAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var to = request.To ?? Today;
        var from = request.From ?? new DateOnly(to.Year, to.Month, 1);
        if (from > to) (from, to) = (to, from);

        var delivered = DeliveredBetween(from, to);
        var summary = await SettlementSummaryAsync(delivered, cancellationToken);
        var start = StartOfDayUtc(from);
        var end = StartOfDayUtc(to.AddDays(1));
        var moneyReceived = await Db.Set<WebApplication1.Models.BasketPayment>().Where(payment =>
                payment.Status == WebApplication1.Models.PaymentReview.Confirmed && payment.ReviewedAt >= start && payment.ReviewedAt < end)
            .SumAsync(payment => payment.Total, cancellationToken);
        var historical = await delivered.Where(o => o.Marketplace == null || o.Marketplace.PaymentId == null)
            .SumAsync(order => order.TotalAmount, cancellationToken);
        var model = new OwnerFinanceModel(summary.Orders, moneyReceived + historical, summary.CommissionDeducted, summary.NetTransferred,
            summary.NetPending, from, to, Today);

        return Page("finance", "ภาพรวมการเงิน", string.Empty, model,
            "เงินรับตามวันตรวจสลิป ค่าคอมและยอดโอนตามออเดอร์ที่ส่งถึงในช่วงที่เลือก");
    }

    private async Task<DashboardPage> RankingAsync(CancellationToken cancellationToken)
    {
        var top = await TopProductsAsync(10, cancellationToken);
        var chart = new ChartData("bar", top.Select(t => Shorten(t.Name)).ToList(), top.Select(t => t.Revenue).ToList(), "รายได้", "baht");
        return Page("ranking", "สินค้าขายดี", string.Empty, new OwnerRankingModel(chart, top), "10 อันดับตามรายได้ ไม่นับออเดอร์ที่ยกเลิก");
    }

    private async Task<(int Orders, decimal Revenue)> PeriodAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var query = Db.Orders.Where(o => o.CreatedAt >= from && o.CreatedAt < to && o.Status != OrderStatus.Cancelled);
        return (await query.CountAsync(cancellationToken), await query.SumAsync(o => o.TotalAmount, cancellationToken));
    }

    /// <summary>
    /// จัดอันดับจากสินค้าที่ยังมีอยู่ โดยเชื่อมกับตาราง products เพื่อคัดรายการที่ถูกลบออก
    /// จัดกลุ่มด้วยรหัสสินค้าเพื่อให้ชื่อเดิมและชื่อใหม่ของสินค้าเดียวกันอยู่ในรายการเดียว
    /// </summary>
    private async Task<List<TopProduct>> TopProductsAsync(int take, CancellationToken cancellationToken) =>
        await (from item in Db.OrderItems.AsNoTracking()
               join product in Db.Products on item.ProductId equals product.Id
               where item.Order!.Status != OrderStatus.Cancelled
               group item by new { product.Id, product.Name } into g
               // เรียงและจำกัดจำนวนจากค่ารวมใน SQL ก่อนสร้าง record เพื่อให้ EF แปลการเรียงลำดับได้
               orderby g.Sum(i => i.TotalPrice) descending
               select new { g.Key.Name, Quantity = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.TotalPrice) })
            .Take(take)
            .Select(x => new TopProduct(x.Name, x.Quantity, x.Revenue))
            .ToListAsync(cancellationToken);

    private static string Shorten(string name) => name.Length > 22 ? name[..22] + "…" : name;
}
