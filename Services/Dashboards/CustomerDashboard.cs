using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.Options;
using WebApplication1.Services.Catalog;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Orders;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

/// <summary>ราคาและสต็อกปัจจุบันสำหรับปุ่มซื้อซ้ำจากประวัติออเดอร์</summary>
public sealed record ProductSnapshot(int Id, string Name, decimal Price, string? ImageUrl, int Stock);

public sealed record CustomerOrdersModel(
    IReadOnlyList<Order> Orders,
    Paged<Order>? Page,
    IReadOnlySet<int> ReviewableProductIds,
    IReadOnlyDictionary<int, ProductSnapshot> Products,
    string Mode);

/// <summary>"คูปองของฉัน": codes still usable, codes already used (with the order and the amount saved).</summary>
public sealed record CustomerCouponsModel(IReadOnlyList<Coupon> Available, IReadOnlyList<UserCoupon> History, decimal TotalSaved);

public sealed class CustomerDashboard : DashboardBase
{
    private readonly IReviewService _reviews;

    public CustomerDashboard(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock, IReviewService reviews)
        : base(db, commerce, currentUser, clock)
    {
        _reviews = reviews;
    }

    public override string Role => Roles.User;

    public override IReadOnlyList<DashboardMenuItem> Menu { get; } =
    [
        new("overview", "ภาพรวม", "bi-grid"),
        new("tracking", "ติดตามพัสดุ", "bi-truck"),
        new("orders", "คำสั่งซื้อทั้งหมด", "bi-bag"),
        new("coupons", "คูปองของฉัน", "bi-ticket-perforated"),
        new("rewards", "เงินสะสมของฉัน", "bi-wallet2")
    ];

    /// <summary>
    /// อ่านคูปองที่เก็บและยังใช้ได้ พร้อมประวัติการใช้คูปองของลูกค้า
    /// เมื่อระบบยกเลิกและลบการใช้จาก user_coupons ลูกค้าจึงใช้สิทธิ์เดิมได้อีกครั้ง
    /// </summary>
    private async Task<CustomerCouponsModel> CouponsAsync(int userId, CancellationToken cancellationToken)
    {
        var now = Clock.GetUtcNow().UtcDateTime;
        var history = await Db.UserCoupons.AsNoTracking().Where(u => u.UserId == userId)
            .OrderByDescending(u => u.UsedAt).ToListAsync(cancellationToken);
        var usedCodes = history.Select(h => h.CouponCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var available = (await Db.Coupons.AsNoTracking()
                .Where(c => c.IsActive && (c.ExpiresAt == null || c.ExpiresAt >= now) &&
                    Db.Set<SavedCoupon>().Any(saved => saved.UserId == userId && saved.CouponId == c.Id))
                .OrderBy(c => c.MinSpend)
                .ToListAsync(cancellationToken))
            .Where(c => !usedCodes.Contains(c.Code))
            .ToList();

        return new CustomerCouponsModel(available, history, history.Sum(h => h.DiscountAmount));
    }

    public override async Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var mine = Db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace).ThenInclude(m => m!.Payment).Include(o => o.Marketplace).ThenInclude(m => m!.Shop)
            .Include(o => o.Shipment).Where(o => o.UserId == userId);
        var openCount = await mine.CountAsync(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled, cancellationToken);
        var menu = Menu.Select(m => m.Key == "tracking" ? m with { Badge = openCount } : m).ToList();        var reviewable = await _reviews.GetReviewableProductIdsAsync(userId, cancellationToken);

        switch (request.Section)
        {
            case "overview":
            {
                var recent = await mine.OrderByDescending(o => o.CreatedAt).Take(3).ToListAsync(cancellationToken);
                var total = await mine.CountAsync(o => o.Status != OrderStatus.Cancelled, cancellationToken);
                var spent = await Db.Orders.Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount, cancellationToken);
                var coupons = await Db.UserCoupons.CountAsync(c => c.UserId == userId, cancellationToken);
                return Page("overview", "ภาพรวม", "_CustomerOrders",
                    new CustomerOrdersModel(recent, null, reviewable, await SnapshotsAsync(recent, cancellationToken), "overview"),
                    greeting: new GreetingBanner($"{Greeting()} คุณ{CurrentUser.DisplayName}",
                        openCount > 0 ? $"มีของกำลังเดินทางมาหาคุณ {openCount} ออเดอร์" : reviewable.Count > 0 ? $"มีสินค้า {reviewable.Count} ชิ้นที่รอรีวิวจากคุณ" : "ช้อปของให้น้องต่อไหม",
                        [new QuickAction("ติดตามพัสดุ", "tracking", true), new QuickAction("คำสั่งซื้อทั้งหมด", "orders")]),
                    metrics:
                    [
                        new MetricTile("กำลังจัดส่ง", openCount.ToString("N0"), "bi-truck"),
                        new MetricTile("สั่งไปแล้ว", total.ToString("N0") + " ครั้ง", "bi-receipt"),
                        new MetricTile("ยอดซื้อที่ได้รับแล้ว", Baht(spent), "bi-cash-stack"),
                        new MetricTile("รอรีวิว", reviewable.Count.ToString("N0"), "bi-star", coupons > 0 ? $"ใช้โค้ดไปแล้ว {coupons} ครั้ง" : null)
                    ],
                    menu: menu);
            }
            case "tracking":
            {
                var open = await mine.Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                    .OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);
                return Page("tracking", "ติดตามพัสดุ", string.Empty,
                    new CustomerOrdersModel(open, null, reviewable, await SnapshotsAsync(open, cancellationToken), "tracking"),
                    "ยกเลิกได้ตอนที่ยัง \"รอร้านยืนยัน\"", menu: menu);
            }
            case "orders":
            {
                var page = await PageAsync(mine.OrderByDescending(o => o.CreatedAt), request.Page, cancellationToken);
                return Page("orders", "คำสั่งซื้อทั้งหมด", string.Empty,
                    new CustomerOrdersModel(page.Items, page, reviewable, await SnapshotsAsync(page.Items, cancellationToken), "orders"),
                    menu: menu);
            }
            case "rewards":
                return Page("rewards", "เงินสะสมของฉัน", string.Empty,
                    await Db.LoyaltyBalanceAsync(CurrentUser.RequireUserId(), cancellationToken));
            case "coupons":
            {
                var coupons = await CouponsAsync(userId, cancellationToken);
                return Page("coupons", "คูปองของฉัน", "_CustomerCoupons", coupons,
                    "กด \"ใช้โค้ด\" เพื่อไปเลือกสินค้า ระบบตรวจเงื่อนไขส่วนลดตอนชำระเงิน โค้ดละ 1 ครั้งต่อบัญชี",
                    metrics:
                    [
                        new MetricTile("โค้ดที่เก็บไว้", coupons.Available.Count.ToString("N0"), "bi-ticket-perforated"),
                        new MetricTile("ใช้ไปแล้ว", coupons.History.Count.ToString("N0") + " ครั้ง", "bi-receipt"),
                        new MetricTile("ประหยัดไปแล้ว", Baht(coupons.TotalSaved), "bi-cash-stack")
                    ],
                    menu: menu);
            }
            default:
                return null;
        }
    }

    private async Task<IReadOnlyDictionary<int, ProductSnapshot>> SnapshotsAsync(IEnumerable<Order> orders, CancellationToken cancellationToken)
    {
        var ids = orders.SelectMany(o => o.Items).Select(i => i.ProductId).Distinct().ToArray();
        return await Db.Products.AsNoTracking().Where(p => ids.Contains(p.Id))
            .Select(p => new ProductSnapshot(p.Id, p.Name, p.Price, p.ImageUrl, p.Stock))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
    }
}
