using WebApplication1.Domain.Orders;

namespace WebApplication1.ViewModels.Dashboard;

/// <summary>Maps an order status to the colour modifier of <c>.lc-status</c>. Kept out of the domain because it is a UI concern.</summary>
public static class StatusTone
{
    public static string CssClass(OrderStatus status) => "lc-status lc-status--" + status switch
    {
        OrderStatus.Pending => "warning",
        OrderStatus.Packing or OrderStatus.Ready => "info",
        OrderStatus.Delivering => "info",
        OrderStatus.Delivered => "success",
        OrderStatus.Cancelled => "danger",
        _ => "neutral"
    };
}
