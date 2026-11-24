namespace WebApplication1.ViewModels.Dashboard;

public static class DashboardRoutes
{
    public static string ActionFor(string? section) => section switch
    {
        "products" => nameof(Controllers.AccountController.Product),
        "packing" => nameof(Controllers.AccountController.Packing),
        "shipped" => nameof(Controllers.AccountController.Shipped),
        "revenue" => nameof(Controllers.AccountController.Revenue),
        "tracking" => nameof(Controllers.AccountController.Tracking),
        "orders" => nameof(Controllers.AccountController.Orders),
        "coupons" => nameof(Controllers.AccountController.Coupons),
        "rewards" => nameof(Controllers.AccountController.Rewards),
        "flash-sales" => nameof(Controllers.AccountController.FlashSales),
        "jobs" => nameof(Controllers.AccountController.Jobs),
        "mine" => nameof(Controllers.AccountController.MyDeliveries),
        "history" => nameof(Controllers.AccountController.History),
        "order-status" => nameof(Controllers.AccountController.OrderStatus),
        "commission" => nameof(Controllers.AccountController.Commission),
        "ledger" => nameof(Controllers.AccountController.Ledger),
        "sales" => nameof(Controllers.AccountController.Sales),
        "finance" => nameof(Controllers.AccountController.Finance),
        "ranking" => nameof(Controllers.AccountController.Ranking),
        "admin-access" => nameof(Controllers.AccountController.AdminAccess),
        "shops" => nameof(Controllers.AccountController.Shops),
        "users" => nameof(Controllers.AccountController.Users),
        "activity" => nameof(Controllers.AccountController.Activity),
        _ => nameof(Controllers.AccountController.Index)
    };
}
