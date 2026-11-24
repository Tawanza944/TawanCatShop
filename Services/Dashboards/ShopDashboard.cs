using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.Options;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

public sealed record ShopOverviewModel(IReadOnlyList<Order> Queue, IReadOnlyList<Product> LowStock, ChartData Revenue);

public sealed record ShopQueueModel(IReadOnlyList<Order> Orders, string? StatusFilter, IReadOnlyDictionary<OrderStatus, int> Counts);

public sealed record ShopProductsModel(Paged<Product> Products, string? Search, string? CategoryFilter, IReadOnlyList<StorefrontCategory> Categories, int LowStockThreshold);

public sealed record ShopRevenueModel(ChartData Revenue, Paged<SettlementRow> Orders, decimal NetTransferred, decimal NetPending, decimal NetAwaiting);

/// <summary>ออเดอร์ที่ออกจากร้านแล้ว <see cref="RiderNames"/> ใช้ค้นชื่อไรเดอร์จากรหัสใน users</summary>
public sealed record ShopShippedModel(Paged<Order> Orders, string? StatusFilter, string? Search,
    IReadOnlyDictionary<OrderStatus, int> Counts, IReadOnlyDictionary<int, string> RiderNames);

public sealed class ShopDashboard : DashboardBase
{
    private static readonly OrderStatus[] QueueStatuses = [OrderStatus.Pending, OrderStatus.Packing, OrderStatus.Ready];
    private static readonly OrderStatus[] ShippedStatuses = [OrderStatus.Delivering, OrderStatus.Delivered];

    public ShopDashboard(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock)
        : base(db, commerce, currentUser, clock)
    {
    }

    private IQueryable<Order> ShopOrders => Db.OrdersFor(CurrentUser.RequireUserId());
    private IQueryable<Product> ShopProducts => Db.ProductsFor(CurrentUser.RequireUserId());

    public override string Role => Roles.Shop;

    public override IReadOnlyList<DashboardMenuItem> Menu { get; } =
    [
        new("overview", "ภาพรวม", "bi-grid"),
        new("packing", "เตรียมออเดอร์", "bi-box-seam"),
        new("shipped", "ส่งแล้ว", "bi-truck"),
        new("products", "สินค้าและสต็อก", "bi-boxes"),
        new("revenue", "รายได้ร้าน", "bi-wallet2")
    ];

    public override async Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var toPack = await ShopOrders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Packing, cancellationToken);
        var menu = Menu.Select(m => m.Key == "packing" ? m with { Badge = toPack } : m).ToList();

        return request.Section switch
        {
            "overview" => await OverviewAsync(toPack, menu, cancellationToken),
            "packing" => Page("packing", "เตรียมออเดอร์", string.Empty, await QueueAsync(request.Filter, cancellationToken),
                "รับออเดอร์ → แพ็กของ → แพ็กเสร็จรอไรเดอร์มารับ", menu: menu),
            "products" => Page("products", "สินค้าและสต็อก", string.Empty, await ProductsAsync(request, cancellationToken),
                "เพิ่ม แก้ไข เติมสต็อก หรือลบสินค้าได้จากหน้านี้", menu: menu),
            "shipped" => Page("shipped", "ออเดอร์ที่ส่งแล้ว", string.Empty, await ShippedAsync(request, cancellationToken),
                "ออเดอร์ที่ไรเดอร์รับไปแล้ว และที่ลูกค้าได้รับของแล้ว", menu: menu),
            "revenue" => Page("revenue", "รายได้ร้าน", string.Empty, await RevenueAsync(request.Page, cancellationToken),
                "สรุปยอดโอนแล้วและยอดที่ยังอยู่ระหว่างดำเนินการ", menu: menu),
            _ => null
        };
    }

    private async Task<DashboardPage> OverviewAsync(int toPack, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken)
    {
        var todayStart = StartOfDayUtc(Today);
        var yesterdayStart = StartOfDayUtc(Today.AddDays(-1));
        var salesToday = await ShopOrders.Where(o => o.CreatedAt >= todayStart && o.Status != OrderStatus.Cancelled).SumAsync(o => o.TotalAmount, cancellationToken);
        var salesYesterday = await ShopOrders.Where(o => o.CreatedAt >= yesterdayStart && o.CreatedAt < todayStart && o.Status != OrderStatus.Cancelled).SumAsync(o => o.TotalAmount, cancellationToken);
        var waitingRider = await ShopOrders.CountAsync(o => o.Status == OrderStatus.Ready, cancellationToken);
        var delivering = await ShopOrders.CountAsync(o => o.Status == OrderStatus.Delivering, cancellationToken);
        var lowStock = await ShopProducts.AsNoTracking().Where(p => p.Stock <= Commerce.LowStockThreshold).OrderBy(p => p.Stock).Take(6).ToListAsync(cancellationToken);
        var queue = await ShopOrders.AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace).ThenInclude(m => m!.Payment)
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Packing)
            .OrderBy(o => o.CreatedAt).Take(5).ToListAsync(cancellationToken);

        var message = toPack == 0 && lowStock.Count == 0
            ? "ไม่มีงานค้าง ร้านพร้อมรับออเดอร์ใหม่"
            : $"วันนี้มีออเดอร์รอแพ็ก {toPack} รายการ และสินค้าใกล้หมด {lowStock.Count} ชิ้น";

        return Page("overview", "ภาพรวม", "_ShopOverview",
            new ShopOverviewModel(queue, lowStock, await RevenueSeriesAsync(7, "ยอดขาย", cancellationToken, scopedOrders: ShopOrders)),
            greeting: new GreetingBanner($"{Greeting()}", message,
                [new QuickAction("แพ็กออเดอร์", "packing", true), new QuickAction("จัดการสินค้า", "products")]),
            metrics:
            [
                new MetricTile("ยอดขายวันนี้", Baht(salesToday), "bi-receipt", Percent(salesToday, salesYesterday), salesToday >= salesYesterday),
                new MetricTile("รอแพ็ก", toPack.ToString("N0"), "bi-box-seam", toPack > 0 ? "ควรแพ็กให้ทันวันนี้" : "ไม่มีงานค้าง"),
                new MetricTile("รอไรเดอร์ / กำลังส่ง", $"{waitingRider} / {delivering}", "bi-truck"),
                new MetricTile("สินค้าใกล้หมด", lowStock.Count.ToString("N0"), "bi-exclamation-triangle", $"เหลือ ≤ {Commerce.LowStockThreshold} ชิ้น", Alert: lowStock.Count > 0)
            ],
            menu: menu);
    }

    private async Task<ShopQueueModel> QueueAsync(string? filter, CancellationToken cancellationToken)
    {
        var counts = await ShopOrders.Where(o => QueueStatuses.Contains(o.Status))
            .GroupBy(o => o.Status).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var query = ShopOrders.AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace).ThenInclude(m => m!.Payment).Where(o => QueueStatuses.Contains(o.Status));
        if (OrderStatusCodes.TryParseCode(filter, out var status) && QueueStatuses.Contains(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query.OrderBy(o => o.Status).ThenBy(o => o.CreatedAt).ToListAsync(cancellationToken);
        return new ShopQueueModel(orders, filter, counts);
    }

    private async Task<ShopProductsModel> ProductsAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var query = ShopProducts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(p => p.Name.Contains(request.Search));
        if (StorefrontCategories.FindByName(request.Filter) is { } category) query = query.Where(p => p.Category == category.Name);
        var page = await PageAsync(query.OrderBy(p => p.Stock > Commerce.LowStockThreshold).ThenByDescending(p => p.Id), request.Page, cancellationToken);
        return new ShopProductsModel(page, request.Search, request.Filter, StorefrontCategories.All, Commerce.LowStockThreshold);
    }

    private async Task<ShopRevenueModel> RevenueAsync(int page, CancellationToken cancellationToken)
    {
        var delivered = ShopOrders.AsNoTracking().Where(order => order.Status == OrderStatus.Delivered);
        var summary = await SettlementSummaryAsync(delivered, cancellationToken);
        return new ShopRevenueModel(await RevenueSeriesAsync(14, "ยอดขาย", cancellationToken, scopedOrders: ShopOrders),
            await SettlementRowsAsync(delivered, page, cancellationToken), summary.NetTransferred, summary.NetPending, summary.NetAwaiting);
    }

    /// <summary>ออเดอร์ของร้านที่กำลังจัดส่งหรือส่งถึงลูกค้าแล้ว</summary>
    private async Task<ShopShippedModel> ShippedAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var counts = await ShopOrders.Where(o => ShippedStatuses.Contains(o.Status))
            .GroupBy(o => o.Status).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var query = ShopOrders.AsNoTracking().Include(o => o.Items).Where(o => ShippedStatuses.Contains(o.Status));
        if (OrderStatusCodes.TryParseCode(request.Filter, out var status) && ShippedStatuses.Contains(status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(o => o.OrderNo.Contains(term) || o.CustomerName.Contains(term) || o.CustomerPhone.Contains(term));
        }

        var page = await PageAsync(query.OrderByDescending(o => o.DeliveredAt ?? o.PickedUpAt ?? o.CreatedAt), request.Page, cancellationToken);

        var riderIds = page.Items.Where(o => o.RiderId != null).Select(o => o.RiderId!.Value).Distinct().ToArray();
        var riders = await Db.Users.AsNoTracking().Where(u => riderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        return new ShopShippedModel(page, request.Filter, request.Search, counts, riders);
    }
}
