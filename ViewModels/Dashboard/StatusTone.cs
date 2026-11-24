using WebApplication1.Domain.Orders;

namespace WebApplication1.ViewModels.Dashboard;

/// <summary>แปลงสถานะออเดอร์เป็นคลาสสีของ <c>.lc-status</c> ซึ่งเป็นเรื่องการแสดงผล</summary>
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
