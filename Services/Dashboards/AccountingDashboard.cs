using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Accounting;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Options;
using WebApplication1.Models;
using WebApplication1.Services.Backoffice;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Dashboard;
using WebApplication1.ViewModels.Shared;

namespace WebApplication1.Services.Dashboards;

public sealed record AccountingOverviewModel(ChartData Income, IReadOnlyList<LedgerLine> RecentLines);

public sealed record AccountingCommissionModel(IReadOnlyList<CommissionRow> Rows, decimal CommissionRate, IReadOnlyList<PayoutQueueRow> Payouts, DateOnly Today);

public sealed record AccountingLedgerModel(LedgerReport Report, DateOnly Today);

public sealed record AccountingOrderStatusModel(Paged<AccountingOrderRow> Orders, string? Search, string? Filter, decimal CommissionRate,
    DateOnly Today, IReadOnlyList<PendingPayment> PendingPayments, PayoutFilter Payout, PayoutSummary Summary);

public sealed record AccountingOrderRow(int Id, string Number, OrderStatus Delivery, decimal Total, decimal? Commission,
    decimal? Net, bool Approved, DateOnly? TransferDate, string? TransferReference, string? ShopName, bool HasPayoutSlip, bool? RiderDelivery, string? PaymentStatus,
    decimal? CustomerGoodsPaid = null, decimal? FlashDiscount = null, decimal? RewardUsed = null, decimal? CouponDiscount = null,
    OrderShipment? Shipment = null)
{
    /// <summary>แสดงขนส่งและเลขติดตามให้ฝ่ายบัญชีตรวจประกอบก่อนอนุมัติยอดโอน</summary>
    public ShipmentInfo? ShipmentInfo => ShipmentInfo.For(RiderDelivery, Delivery, Shipment);

    public bool Transferred => TransferDate.HasValue;

    public bool CanApprove => Stage == PayoutStage.AwaitingApproval;
    public bool CanRecordTransfer => Stage == PayoutStage.Ready;

    private PayoutStage Stage => PayoutStages.Resolve(Delivery, Approved, Transferred);
    public string FinanceStatus => Stage.Label();
}

public sealed class AccountingDashboard : DashboardBase
{
    private readonly IAccountingService _accounting;
    private readonly PayoutSummaryReader _payoutSummary = new();

    public AccountingDashboard(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock, IAccountingService accounting)
        : base(db, commerce, currentUser, clock)
    {
        _accounting = accounting;
    }

    public override string Role => Roles.Accounting;

    public override IReadOnlyList<DashboardMenuItem> Menu { get; } =
    [
        new("overview", "ภาพรวม", "bi-grid"),
        new("order-status", "สถานะออเดอร์", "bi-list-check"),
        new("ledger", "สมุดรายรับรายจ่าย", "bi-journal-text")
    ];

    public override async Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        // เลขที่เมนูนับออเดอร์ยังไม่โอนทั้งระบบ ส่วนกล่องสรุปใช้เงื่อนไขค้นหาของหน้านี้
        var unpaid = await Db.Orders.WherePayout(PayoutFilter.Unpaid).CountAsync(cancellationToken);
        var menu = Menu.Select(item => item.Key == "order-status" ? item with { Badge = unpaid } : item).ToList();
        if (request.Section == "order-status")
        {
            return await OrderStatusAsync(request, menu, cancellationToken);
        }

        var awaiting = await _accounting.GetAwaitingCommissionAsync(cancellationToken);

        switch (request.Section)
        {
            case "overview":
            {
                var monthStart = new DateOnly(Today.Year, Today.Month, 1);
                var report = await _accounting.GetLedgerAsync(monthStart, Today, cancellationToken);
                var s = report.Summary;
                return Page("overview", "ภาพรวม", "_AccountingOverview",
                    new AccountingOverviewModel(await RevenueSeriesAsync(7, "เงินเข้า", cancellationToken, deliveredOnly: true), report.Lines.Take(6).ToList()),
                    greeting: new GreetingBanner($"{Greeting()} ฝ่ายบัญชี",
                        awaiting.Count > 0 ? $"มีออเดอร์รอหักคอม {awaiting.Count} รายการ" : "หักคอมครบทุกออเดอร์แล้ว",
                        [new QuickAction("ดูสถานะออเดอร์", "order-status", true), new QuickAction("เปิดสมุดบัญชี", "ledger")]),
                    metrics:
                    [
                        new MetricTile("รายได้ค่าคอมเดือนนี้", $"฿{s.CommissionEarned:N2}", "bi-cash-stack"),
                        new MetricTile("ค่าใช้จ่ายเว็บเดือนนี้", $"฿{s.PlatformExpenses:N2}", "bi-receipt"),
                        new MetricTile("รายได้สุทธิของ LoveCat", $"฿{s.PlatformNet:N2}", "bi-bar-chart", "ค่าคอม − ค่าใช้จ่ายเว็บ", Alert: s.PlatformNet < 0),
                        new MetricTile("ยอดค้างโอนร้านค้า", $"฿{s.OutstandingPayout:N2}", "bi-box-seam")
                    ], menu: menu);
            }
            case "commission":
                return Page("commission", "หักค่าคอมมิชชัน", string.Empty, new AccountingCommissionModel(awaiting, Commerce.CommissionRate,
                    await _accounting.GetPayoutQueueAsync(cancellationToken), Today),
                    $"ออเดอร์ที่ส่งถึงแล้ว หัก {Commerce.CommissionRate:P0} ก่อนโอนให้ร้าน", menu: menu);
            case "ledger":
            {
                var to = request.To ?? Today;
                var from = request.From ?? new DateOnly(to.Year, to.Month, 1);
                if (from > to) (from, to) = (to, from);
                return Page("ledger", "สมุดรายรับรายจ่าย", string.Empty,
                    new AccountingLedgerModel(await _accounting.GetLedgerAsync(from, to, cancellationToken), Today),
                    "ทุกออเดอร์ที่ส่งสำเร็จลงบัญชีให้อัตโนมัติ รายการอื่นบันทึกเองได้", menu: menu);
            }
            default:
                return null;
        }
    }
    private async Task<DashboardPage> OrderStatusAsync(DashboardRequest request, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim();
        var orders = Db.Orders.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
            orders = orders.Where(o => o.OrderNo.Contains(search));

        var filter = OrderStatusCodes.TryParseCode(request.Filter, out var delivery) ? delivery.ToCode() : null;
        if (filter is not null)
            orders = orders.Where(o => o.Status == delivery);

        var summary = await _payoutSummary.ReadAsync(orders, cancellationToken);
        var payout = Enum.IsDefined(request.Payout) ? request.Payout : PayoutFilter.All;
        var orderRows = from o in orders.WherePayout(payout)
                        join incentive in Db.Set<OrderIncentives>() on o.Id equals incentive.OrderId into incentives
                        from incentive in incentives.DefaultIfEmpty()
                        orderby o.CreatedAt descending, o.Id descending
                        select new AccountingOrderRow(o.Id, o.OrderNo, o.Status, o.Subtotal,
                (decimal?)o.ShopPayout!.Commission, (decimal?)o.ShopPayout!.NetAmount,
                o.ShopPayout != null, o.ShopPayout!.TransferDate, o.ShopPayout!.TransferReference, o.Marketplace!.Shop.Name, Db.Set<WebApplication1.Models.BankSlip>().Any(slip => slip.PayoutOrderId == o.Id),
                o.Marketplace == null ? null : (bool?)o.Marketplace.RiderDelivery, o.Marketplace!.Payment!.Status,
                (decimal?)incentive!.EligibleSpend, (decimal?)incentive!.FlashDiscount, (decimal?)incentive!.RewardUsed, (decimal?)incentive!.CouponDiscount,
                o.Shipment);
        var rows = await PageAsync(orderRows, request.Page, cancellationToken);

        var payments = await (from payment in Db.Set<WebApplication1.Models.BasketPayment>().AsNoTracking()
                              join customer in Db.Users on payment.UserId equals customer.Id
                              where payment.Status == PaymentReview.Pending || payment.Status == PaymentReview.Rejected
                              orderby payment.CreatedAt, payment.Id
                              select new PendingPayment(payment.Id, payment.Slip!.Id, payment.Total, payment.CreatedAt, customer.Username, payment.Status))
                             .ToListAsync(cancellationToken);
        return Page("order-status", "สถานะออเดอร์", string.Empty,
            new AccountingOrderStatusModel(rows, search, filter, Commerce.CommissionRate, Today, payments, payout, summary), menu: menu);
    }
}
