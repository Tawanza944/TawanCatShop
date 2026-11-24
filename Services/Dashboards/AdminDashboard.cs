using Microsoft.EntityFrameworkCore;
using WebApplication1.Domain.Orders;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Identity;
using WebApplication1.Models;
using WebApplication1.Options;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Backoffice;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

/// <summary>ข้อมูลผู้ใช้หนึ่งแถวในหลังบ้าน ใช้พร็อพเพอร์ตีแบบ init เพื่อประกอบ LINQ ของ EF Core</summary>
public sealed record AdminUserRow
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Role { get; init; } = string.Empty;
    public bool Suspended { get; init; }
    public DateTime? CreatedAt { get; init; }
    public bool IsSelf { get; init; }
}

public sealed record AdminOverviewModel(ChartData UsersByRole, IReadOnlyList<AdminUserRow> LatestUsers, IReadOnlyList<AdminActivity> LatestActivity);

public sealed record AdminUsersModel(Paged<AdminUserRow> Users, IReadOnlyList<string> Roles, string? Search, string? RoleFilter);

public sealed record AdminCouponsModel(IReadOnlyList<Coupon> Coupons, IReadOnlyDictionary<string, int> UsageCounts, IReadOnlyList<StorefrontCategory> Categories);

public sealed record AdminActivityModel(Paged<AdminActivity> Activities, string? Search);

public sealed class AdminDashboard : DashboardBase
{
    public AdminDashboard(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock)
        : base(db, commerce, currentUser, clock)
    {
    }

    public sealed record ShopAccount(int Id, string Username, string Role);
    public sealed record ShopAdminModel(IReadOnlyList<WebApplication1.Models.MarketplaceShop> Shops, IReadOnlyList<ShopAccount> Accounts);

    public override string Role => Roles.Admin;

    public override IReadOnlyList<DashboardMenuItem> Menu { get; } =
    [
        new("overview", "ภาพรวม", "bi-grid"),
        new("shops", "ร้านค้าและพื้นที่ไรเดอร์", "bi-shop"),
        new("users", "ผู้ใช้และสิทธิ์", "bi-people"),
        new("coupons", "โค้ดส่วนลด", "bi-ticket-perforated"),
        new("flash-sales", "Flash Sale", "bi-lightning-charge"),
        new("activity", "ประวัติการดูแลระบบ", "bi-clock-history")
    ];

    public override async Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken) => request.Section switch
    {
        "overview" => await OverviewAsync(cancellationToken),
        "shops" => await ShopsAsync(cancellationToken),
        "flash-sales" => await FlashSalesAsync(cancellationToken),
        "users" => Page("users", "ผู้ใช้และสิทธิ์", string.Empty, await UsersAsync(request, cancellationToken), "แก้ข้อมูล เปลี่ยนบทบาท หรือระงับบัญชี"),
        "coupons" => Page("coupons", "โค้ดส่วนลด", "_AdminCoupons", await CouponsAsync(cancellationToken), "โค้ดที่เปิดอยู่จะแสดงในหน้าโปรโมชั่นทันที"),
        "activity" => Page("activity", "ประวัติการดูแลระบบ", string.Empty,
            new AdminActivityModel(await PageAsync(Db.AdminActivities.AsNoTracking()
                .Where(a => string.IsNullOrEmpty(request.Search) || a.Description.Contains(request.Search) || a.Actor.Contains(request.Search))
                .OrderByDescending(a => a.Id), request.Page, cancellationToken), request.Search)),
        _ => null
    };

    private async Task<DashboardPage> FlashSalesAsync(CancellationToken token)
    {
        var products = await Db.Products.AsNoTracking().Where(p => p.Shop != null)
            .OrderBy(p => p.Shop!.Shop.Name).ThenBy(p => p.Name)
            .Select(p => new FlashProduct(p.Id, p.Name, p.Shop!.Shop.Name, p.Price)).ToListAsync(token);
        var sales = await (from sale in Db.Set<FlashSale>().AsNoTracking()
                           join product in Db.Products on sale.ProductId equals product.Id
                           where product.Shop != null
                           orderby sale.StartsAt descending
                           select new FlashListing(product.Name, product.Shop!.Shop.Name, sale.Price, sale.StartsAt, sale.EndsAt, sale.Quota, sale.Sold))
            .ToListAsync(token);
        return Page("flash-sales", "Flash Sale", string.Empty, new FlashAdminPage(products, sales, Clock.GetUtcNow().UtcDateTime));
    }

    private async Task<DashboardPage> ShopsAsync(CancellationToken token)
    {
        var shops = await Db.Set<MarketplaceShop>().Where(shop => shop.Name != MarketplaceShop.PlatformName)
            .OrderBy(shop => shop.Name).ToListAsync(token);
        var accounts = await (from staff in Db.Staff
                              join user in Db.Users on staff.UserId equals user.Id
                              join role in Db.Roles on staff.RoleId equals role.Id
                              where role.Name == Roles.Shop || role.Name == Roles.Raider
                              orderby user.Username
                              select new ShopAccount(user.Id, user.Username, role.Name)).ToListAsync(token);
        return Page("shops", "ร้านค้าและพื้นที่ไรเดอร์", string.Empty, new ShopAdminModel(shops, accounts));
    }

    private async Task<DashboardPage> OverviewAsync(CancellationToken cancellationToken)
    {
        var weekAgo = Clock.GetUtcNow().UtcDateTime.AddDays(-7);
        var todayStart = StartOfDayUtc(Today);
        var users = await Db.Users.CountAsync(cancellationToken);
        var newUsers = await Db.Users.CountAsync(u => u.CreatedAt >= weekAgo, cancellationToken);
        var ordersToday = await Db.Orders.CountAsync(o => o.CreatedAt >= todayStart, cancellationToken);
        var activeCoupons = await Db.Coupons.CountAsync(c => c.IsActive, cancellationToken);
        var suspended = await Db.Staff.CountAsync(s => s.Status == StaffStatus.Suspended, cancellationToken);

        var roleCounts = await MembersQuery().GroupBy(m => m.Role).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        var chart = new ChartData("bar",
            Roles.All.Select(r => RoleProfiles.For(r).ThaiName).ToList(),
            Roles.All.Select(r => (decimal)(roleCounts.FirstOrDefault(c => c.Key == r)?.Count ?? 0)).ToList(),
            "จำนวนผู้ใช้");

        var latest = await MembersQuery().OrderByDescending(m => m.Id).Take(5).ToListAsync(cancellationToken);
        var activity = await Db.AdminActivities.AsNoTracking().OrderByDescending(a => a.Id).Take(5).ToListAsync(cancellationToken);

        return Page("overview", "ภาพรวม", "_AdminOverview", new AdminOverviewModel(chart, latest, activity),
            greeting: new GreetingBanner($"{Greeting()} คุณ{CurrentUser.DisplayName}",
                newUsers > 0 ? $"สัปดาห์นี้มีสมาชิกใหม่ {newUsers} คน" : "สัปดาห์นี้ยังไม่มีสมาชิกใหม่",
                [new QuickAction("จัดการผู้ใช้", "users", true), new QuickAction("สร้างโค้ดส่วนลด", "coupons")]),
            metrics:
            [
                new MetricTile("ผู้ใช้ทั้งหมด", users.ToString("N0"), "bi-people", $"ระงับอยู่ {suspended} บัญชี"),
                new MetricTile("สมาชิกใหม่ 7 วัน", newUsers.ToString("N0"), "bi-star"),
                new MetricTile("ออเดอร์วันนี้", ordersToday.ToString("N0"), "bi-receipt"),
                new MetricTile("โค้ดที่เปิดอยู่", activeCoupons.ToString("N0"), "bi-ticket-perforated")
            ]);
    }

    private async Task<AdminUsersModel> UsersAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var query = MembersQuery();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(m => m.Name.Contains(term) || m.Username.Contains(term) || m.Email.Contains(term) || (m.Phone != null && m.Phone.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            query = query.Where(m => m.Role == request.Filter);
        }

        var page = await PageAsync(query.OrderByDescending(m => m.Id), request.Page, cancellationToken);
        return new AdminUsersModel(page, Roles.All, request.Search, request.Filter);
    }

    private async Task<AdminCouponsModel> CouponsAsync(CancellationToken cancellationToken)
    {
        var coupons = await Db.Coupons.AsNoTracking().OrderByDescending(c => c.IsActive).ThenBy(c => c.Code).ToListAsync(cancellationToken);
        var usage = await Db.UserCoupons.AsNoTracking().GroupBy(u => u.CouponCode).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        return new AdminCouponsModel(coupons, usage, StorefrontCategories.All);
    }

    private IQueryable<AdminUserRow> MembersQuery()
    {
        var selfId = CurrentUser.UserId;
        return from user in Db.Users.AsNoTracking()
               join staff in Db.Staff on user.Id equals staff.UserId into memberships
               from membership in memberships.DefaultIfEmpty()
               join role in Db.Roles on membership.RoleId equals role.Id into roles
               from assigned in roles.DefaultIfEmpty()
               // กำหนดพร็อพเพอร์ตีรายตัวเพื่อให้ EF Core ต่อ Where, OrderBy หรือ GroupBy กับ Query ได้
               select new AdminUserRow
               {
                   Id = user.Id,
                   Username = user.Username,
                   Name = user.Name,
                   Email = user.Email,
                   Phone = user.PhoneNumber,
                   Role = assigned == null ? Roles.User : assigned.Name,
                   Suspended = membership != null && membership.Status == StaffStatus.Suspended,
                   CreatedAt = user.CreatedAt,
                   IsSelf = user.Id == selfId
               };
    }
}
