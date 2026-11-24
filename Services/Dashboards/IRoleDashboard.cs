using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Services.Dashboards;

/// <summary>
/// Strategy for one role's back office: its menu and how each section is built.
/// AccountController picks the strategy that matches the signed-in user's role.
/// </summary>
public interface IRoleDashboard
{
    string Role { get; }

    IReadOnlyList<DashboardMenuItem> Menu { get; }

    Task<DashboardPage?> BuildAsync(DashboardRequest request, CancellationToken cancellationToken);
}
