using System.Globalization;
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

/// <summary>ฟังก์ชันใช้ร่วมกันของ Dashboard เช่น วันตามเวลาไทย การแบ่งหน้า และข้อมูลกราฟรายได้</summary>
public abstract class DashboardBase : IRoleDashboard
{
    protected const int PageSize = 10;
    protected static readonly CultureInfo Thai = new("th-TH");

    protected DashboardBase(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock)
    {
        Db = db;
        Commerce = commerce.Value;
        CurrentUser = currentUser;
        Clock = clock;
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(Commerce.TimeZoneId);
    }

    protected AppDbContext Db { get; }

    protected CommerceOptions Commerce { get; }

    protected ICurrentUserService CurrentUser { get; }

    protected TimeProvider Clock { get; }

    protected TimeZoneInfo TimeZone { get; }

    public abstract string Role { get; }

    public abstract IReadOnlyList<DashboardMenuItem> Menu { get; }

    public abstract Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken);

    protected DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Clock.GetUtcNow(), TimeZone).DateTime);

    /// <summary>เวลา UTC ที่ตรงกับต้นวันตามปฏิทินไทยที่ระบุ</summary>
    protected DateTime StartOfDayUtc(DateOnly day) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), TimeZone);

    protected DateOnly LocalDay(DateTime utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone));

    protected string Greeting()
    {
        var hour = TimeZoneInfo.ConvertTime(Clock.GetUtcNow(), TimeZone).Hour;
        return hour < 12 ? "อรุณสวัสดิ์" : hour < 17 ? "สวัสดีตอนบ่าย" : "สวัสดีตอนเย็น";
    }

    protected static string Baht(decimal value) => $"฿{value:N0}";

    protected static string Percent(decimal current, decimal previous) =>
        previous == 0 ? (current > 0 ? "ใหม่" : "เท่าเดิม") : $"{(current - previous) / previous:+0%;-0%;0%} จากช่วงก่อน";

    protected DashboardPage Page(string section, string title, string partial, object? model, string? subtitle = null,
        GreetingBanner? greeting = null, IReadOnlyList<MetricTile>? metrics = null, IReadOnlyList<DashboardMenuItem>? menu = null) => new()
    {
        Role = RoleProfiles.For(Role),
        Menu = menu ?? Menu,
        Section = section,
        Title = title,
        Subtitle = subtitle,
        Greeting = greeting,
        Metrics = metrics ?? [],
        PartialName = partial,
        Model = model,
        DisplayName = CurrentUser.DisplayName ?? string.Empty
    };

    protected static async Task<Paged<T>> PageAsync<T>(IQueryable<T> query, int page, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        var current = Math.Clamp(page, 1, pageCount);
        var items = await query.Skip((current - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken);
        return new Paged<T>(items, current, pageCount, total);
    }

    /// <summary>รายได้จากออเดอร์ที่ไม่ได้ยกเลิกในช่วง <paramref name="days"/> วันตามเวลาไทย เรียงจากเก่าไปใหม่</summary>
    protected async Task<ChartData> RevenueSeriesAsync(int days, string label, CancellationToken cancellationToken, bool deliveredOnly = false, IQueryable<Order>? scopedOrders = null)
    {
        var firstDay = Today.AddDays(-(days - 1));
        var since = StartOfDayUtc(firstDay);
        var orders = await (scopedOrders ?? Db.Orders).AsNoTracking()
            .Where(o => o.CreatedAt >= since && o.Status != OrderStatus.Cancelled && (!deliveredOnly || o.Status == OrderStatus.Delivered))
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync(cancellationToken);

        var byDay = orders.GroupBy(o => LocalDay(o.CreatedAt)).ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
        var range = Enumerable.Range(0, days).Select(i => firstDay.AddDays(i)).ToList();
        return new ChartData("bar",
            range.Select(d => d == Today ? "วันนี้" : d.ToString(days > 10 ? "d/M" : "ddd", Thai)).ToList(),
            range.Select(d => byDay.GetValueOrDefault(d)).ToList(),
            label, "baht");
    }

    // ---------- ยอดที่ร้านได้รับหลังฝ่ายบัญชีหักค่าคอม ----------

    /// <summary>ออเดอร์ที่ส่งถึงในช่วงวันตามเวลาไทย โดยระบุวันเริ่มหรือวันสิ้นสุดอย่างใดอย่างหนึ่งได้</summary>
    protected IQueryable<Order> DeliveredBetween(DateOnly? from, DateOnly? to)
    {
        var query = Db.Orders.AsNoTracking().Where(o => o.Status == OrderStatus.Delivered);
        if (from is { } fromDay)
        {
            var start = StartOfDayUtc(fromDay);
            query = query.Where(o => (o.DeliveredAt ?? o.CreatedAt) >= start);
        }

        if (to is { } toDay)
        {
            var end = StartOfDayUtc(toDay.AddDays(1));
            query = query.Where(o => (o.DeliveredAt ?? o.CreatedAt) < end);
        }

        return query;
    }

    protected async Task<SettlementSummary> SettlementSummaryAsync(IQueryable<Order> delivered, CancellationToken cancellationToken)
    {
        var rate = Commerce.CommissionRate;
        var totals = (await delivered.GroupBy(_ => 1)
            .Select(g => new
            {
                Orders = g.Count(),
                Sales = g.Sum(o => o.TotalAmount),
                Commission = g.Where(o => o.CommissionDeducted)
                    .Sum(o => o.ShopPayout != null ? o.ShopPayout.Commission : Math.Round(o.Subtotal * rate, 2)),
                NetTransferred = g.Where(o => o.ShopPayout != null && o.ShopPayout.TransferDate != null)
                    .Sum(o => o.ShopPayout!.NetAmount),
                NetPending = g.Where(o => o.CommissionDeducted && (o.ShopPayout == null || o.ShopPayout.TransferDate == null))
                    .Sum(o => o.ShopPayout != null ? o.ShopPayout.NetAmount : o.Subtotal - Math.Round(o.Subtotal * rate, 2)),
                NetAwaiting = g.Where(o => !o.CommissionDeducted).Sum(o => o.Subtotal - Math.Round(o.Subtotal * rate, 2)),
                AwaitingCount = g.Count(o => !o.CommissionDeducted)
            }).ToListAsync(cancellationToken)).SingleOrDefault();

        return totals is null
            ? new SettlementSummary(0, 0, 0, 0, 0, 0, 0)
            : new SettlementSummary(totals.Orders, totals.Sales, totals.Commission, totals.NetTransferred,
                totals.NetPending, totals.NetAwaiting, totals.AwaitingCount);
    }

    protected Task<Paged<SettlementRow>> SettlementRowsAsync(IQueryable<Order> delivered, int page, CancellationToken cancellationToken)
    {
        var rate = Commerce.CommissionRate;
        return PageAsync(delivered.OrderByDescending(o => o.DeliveredAt ?? o.CreatedAt).ThenByDescending(o => o.Id)
            .Select(o => new SettlementRow(o.Id, o.OrderNo, o.CustomerName, o.DeliveredAt ?? o.CreatedAt, o.Subtotal,
                o.ShopPayout != null ? o.ShopPayout.Commission : Math.Round(o.Subtotal * rate, 2),
                o.ShopPayout != null ? o.ShopPayout.NetAmount : o.Subtotal - Math.Round(o.Subtotal * rate, 2),
                o.CommissionDeducted, o.ShopPayout != null, o.ShopPayout != null && o.ShopPayout.TransferDate != null, Db.Set<BankSlip>().Any(slip => slip.PayoutOrderId == o.Id))),
            page, cancellationToken);
    }
}

public sealed record SettlementRow(int Id, string OrderNo, string Customer, DateTime At, decimal Total, decimal Commission,
    decimal Net, bool CommissionDeducted, bool Approved, bool Transferred, bool HasSlip)
{
    public string Status => PayoutRules.StatusLabel(CommissionDeducted, Approved, Transferred);
}

public sealed record SettlementSummary(int Orders, decimal Sales, decimal CommissionDeducted,
    decimal NetTransferred, decimal NetPending, decimal NetAwaiting, int AwaitingCount);
