using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

/// <summary>
/// กำหนดเมนูและวิธีเตรียมข้อมูลหลังบ้านของแต่ละบทบาทด้วย Strategy
/// AccountController เลือก Dashboard ตามบทบาทของผู้ใช้ที่ล็อกอิน
/// </summary>
public interface IRoleDashboard
{
    string Role { get; }

    IReadOnlyList<DashboardMenuItem> Menu { get; }

    Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken);
}
