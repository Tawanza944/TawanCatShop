using WebApplication1.Domain.Accounting;

namespace WebApplication1.ViewModels.Dashboard;

public sealed class DashboardQuery
{
    public int PageNumber { get; set; } = 1;
    public string? Search { get; set; }
    public string? Filter { get; set; }
    public PayoutFilter Payout { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
