namespace WebApplication1.Domain.Orders;

/// <summary>
/// วงจรสถานะออเดอร์ เก็บใน <c>orders.status</c> ด้วยรหัสตัวพิมพ์เล็กจาก <see cref="OrderStatusCodes"/>
/// </summary>
public enum OrderStatus
{
    Pending,
    Packing,
    Ready,
    Delivering,
    Delivered,
    Cancelled
}
