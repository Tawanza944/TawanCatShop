using WebApplication1.Domain.Identity;

namespace WebApplication1.ViewModels.Dashboard;

public sealed record DashboardRequest(int UserId, string Section, int Page, string? Search, string? Filter, DateOnly? From, DateOnly? To,
    WebApplication1.Domain.Accounting.PayoutFilter Payout = WebApplication1.Domain.Accounting.PayoutFilter.All);

public sealed record PendingPayment(Guid Id, Guid SlipId, decimal Total, DateTime CreatedAt, string Username, string Status);

public sealed record DashboardMenuItem(string Key, string Title, string Icon, int Badge = 0);

public sealed record DashboardSidebar(RoleProfile Role, IReadOnlyList<DashboardMenuItem> Menu, string Section, string DisplayName)
{
    public const string ProfileSection = "profile";

    public bool HasOrdersPage => Menu.Any(item => item.Key == "orders");
}

/// <summary>การ์ดตัวเลข โดย <see cref="Icon"/> เก็บชื่อคลาสของ Bootstrap Icons</summary>
public sealed record MetricTile(string Label, string Value, string Icon, string? Note = null, bool? Up = null, bool Alert = false);

public sealed record QuickAction(string Label, string Section, bool Primary = false);

public sealed record GreetingBanner(string Title, string Message, IReadOnlyList<QuickAction> Actions);

/// <summary>ข้อมูลกราฟ Chart.js ที่ใช้ใน wwwroot/js/app/charts.js</summary>
public sealed record ChartData(string Kind, IReadOnlyList<string> Labels, IReadOnlyList<decimal> Values, string SeriesLabel, string Format = "number");

public sealed record Paged<T>(IReadOnlyList<T> Items, int Page, int PageCount, int Total)
{
    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < PageCount;
}

/// <summary>ข้อมูลที่หน้า Dashboard ใช้แสดงหนึ่งส่วนสำหรับบทบาทปัจจุบัน</summary>
public sealed class DashboardPage
{
    public required RoleProfile Role { get; init; }

    public required IReadOnlyList<DashboardMenuItem> Menu { get; init; }

    public required string Section { get; init; }

    public required string Title { get; init; }

    public string? Subtitle { get; init; }

    public GreetingBanner? Greeting { get; init; }

    public IReadOnlyList<MetricTile> Metrics { get; init; } = [];

    /// <summary>ชื่อ Partial ใต้ Views/Account/Sections ที่ใช้แสดง <see cref="Model"/></summary>
    public required string PartialName { get; init; }

    public object? Model { get; init; }

    public required string DisplayName { get; init; }
}

/// <summary>ข้อมูลลิงก์แบ่งหน้า โดยเก็บคำค้น ตัวกรอง และช่วงวันที่ปัจจุบัน</summary>
public sealed record PagerModel(string Section, int Page, int PageCount, int Total, string? Search = null, string? Filter = null)
{
    public WebApplication1.Domain.Accounting.PayoutFilter? Payout { get; init; }
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    /// <summary>จัดวันที่แบบ ISO yyyy-MM-dd สำหรับลิงก์และ &lt;input type="date"&gt; โดยใช้รูปแบบเดียวกันทุกเครื่อง</summary>
    public static string? Iso(DateOnly? date) => date?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    public static PagerModel From<T>(Paged<T> paged, string section, string? search = null, string? filter = null) =>
        new(section, paged.Page, paged.PageCount, paged.Total, search, filter);
}
