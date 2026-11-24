using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.Options;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

public sealed record RaiderJobsModel(IReadOnlyList<Order> Orders, string Mode, decimal FeePerJob);

/// <summary>Delivered jobs of one rider; <see cref="From"/>/<see cref="To"/> are the optional date filter (Thai calendar days).</summary>
public sealed record RaiderHistoryModel(Paged<Order> Orders, decimal FeePerJob, decimal TotalEarned, DateOnly? From, DateOnly? To, DateOnly Today);

public sealed class RaiderDashboard : DashboardBase
{
    public RaiderDashboard(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock)
        : base(db, commerce, currentUser, clock)
    {
    }

    public override string Role => Roles.Raider;

    public override IReadOnlyList<DashboardMenuItem> Menu { get; } =
    [
        new("overview", "ภาพรวม", "bi-grid"),
        new("jobs", "งานรอรับ", "bi-box-seam"),
        new("mine", "กำลังส่ง", "bi-bicycle"),
        new("history", "ประวัติและรายได้", "bi-clock-history")
    ];

    public override async Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        var riderId = request.UserId;
        var ready = ReadyFor(riderId);
        var available = await ready.CountAsync(cancellationToken);
        var mine = await Db.Orders.CountAsync(o => o.Status == OrderStatus.Delivering && o.RiderId == riderId, cancellationToken);
        var menu = Menu.Select(m => m.Key switch
        {
            "jobs" => m with { Badge = available },
            "mine" => m with { Badge = mine },
            _ => m
        }).ToList();

        return request.Section switch
        {
            "overview" => await OverviewAsync(riderId, available, mine, menu, cancellationToken),
            "jobs" => await JobsAsync(riderId, ready, menu, cancellationToken),
            "mine" => Page("mine", "กำลังส่ง", string.Empty, new RaiderJobsModel(
                await Db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace).ThenInclude(link => link!.Shop).Where(o => o.Status == OrderStatus.Delivering && o.RiderId == riderId).OrderBy(o => o.PickedUpAt).ToListAsync(cancellationToken),
                "mine", Commerce.RiderFeePerDelivery), "ส่งถึงมือลูกค้าแล้วกด \"ส่งสำเร็จ\"", menu: menu),
            "history" => await HistoryAsync(riderId, request, menu, cancellationToken),
            _ => null
        };
    }

    private async Task<DashboardPage> JobsAsync(int riderId, IQueryable<Order> ready, IReadOnlyList<DashboardMenuItem> menu, CancellationToken token)
    {
        var province = await Db.Set<RiderProvince>().Where(p => p.UserId == riderId).Select(p => p.ProvinceCode).SingleOrDefaultAsync(token);
        var orders = await ready.AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace)
            .ThenInclude(link => link!.Shop).OrderBy(o => o.UpdatedAt).ToListAsync(token);
        var name = DeliveryCoverage.ProvinceName(province);
        return Page("jobs", "งานรอรับ", string.Empty, new RaiderJobsModel(orders, "jobs", Commerce.RiderFeePerDelivery),
            name is null ? "บัญชีนี้ยังไม่ได้รับมอบหมายจังหวัด" : $"จังหวัดที่รับผิดชอบ: {name}", menu: menu);
    }

    private IQueryable<Order> ReadyFor(int riderId) => Db.Orders.Where(order =>
        order.Status == OrderStatus.Ready && order.RiderId == null &&
        order.Marketplace != null && order.Marketplace.RiderDelivery &&
        (order.Marketplace.PaymentId == null || order.Marketplace.Payment!.Status == PaymentReview.Confirmed) &&
        Db.Set<RiderProvince>().Any(p => p.UserId == riderId && p.ProvinceCode == order.Marketplace.ProvinceCode));

    private async Task<DashboardPage> OverviewAsync(int riderId, int available, int mine, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken)
    {
        var todayStart = StartOfDayUtc(Today);
        var deliveredToday = await Db.Orders.CountAsync(o => o.RiderId == riderId && o.Status == OrderStatus.Delivered && o.DeliveredAt >= todayStart, cancellationToken);
        var next = await Db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace).ThenInclude(link => link!.Shop)
            .Where(o => (o.Status == OrderStatus.Delivering && o.RiderId == riderId) || ReadyFor(riderId).Any(ready => ready.Id == o.Id))
            .OrderByDescending(o => o.Status == OrderStatus.Delivering).ThenBy(o => o.UpdatedAt).Take(4).ToListAsync(cancellationToken);

        return Page("overview", "ภาพรวม", "_RaiderJobs", new RaiderJobsModel(next, "overview", Commerce.RiderFeePerDelivery),
            greeting: new GreetingBanner($"{Greeting()} คุณ{CurrentUser.DisplayName}",
                available > 0 ? $"มีงานรอรับ {available} งาน ขับขี่ปลอดภัยนะ" : "ยังไม่มีงานใหม่ พักก่อนได้",
                [new QuickAction("ดูงานรอรับ", "jobs", true), new QuickAction("งานที่กำลังส่ง", "mine")]),
            metrics:
            [
                new MetricTile("งานรอรับ", available.ToString("N0"), "bi-box-seam"),
                new MetricTile("กำลังส่ง", mine.ToString("N0"), "bi-bicycle"),
                new MetricTile("ส่งแล้ววันนี้", deliveredToday.ToString("N0"), "bi-truck"),
                new MetricTile("รายได้วันนี้", Baht(deliveredToday * Commerce.RiderFeePerDelivery), "bi-cash-stack", $"งานละ {Baht(Commerce.RiderFeePerDelivery)}")
            ],
            menu: menu);
    }

    private async Task<DashboardPage> HistoryAsync(int riderId, DashboardRequest request, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken)
    {
        var (from, to) = (request.From, request.To);
        if (from > to) (from, to) = (to, from);

        var query = Db.Orders.AsNoTracking().Where(o => o.RiderId == riderId && o.Status == OrderStatus.Delivered);
        if (from is { } fromDay)
        {
            var start = StartOfDayUtc(fromDay);
            query = query.Where(o => o.DeliveredAt >= start);
        }

        if (to is { } toDay)
        {
            var end = StartOfDayUtc(toDay.AddDays(1));
            query = query.Where(o => o.DeliveredAt < end);
        }

        var count = await query.CountAsync(cancellationToken);
        var orders = await PageAsync(query.OrderByDescending(o => o.DeliveredAt), request.Page, cancellationToken);
        return Page("history", "ประวัติและรายได้", string.Empty,
            new RaiderHistoryModel(orders, Commerce.RiderFeePerDelivery, count * Commerce.RiderFeePerDelivery, from, to, Today), menu: menu);
    }
}
