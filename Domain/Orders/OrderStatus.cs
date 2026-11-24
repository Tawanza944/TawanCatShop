namespace WebApplication1.Domain.Orders;

/// <summary>
/// Lifecycle of an order. Stored in <c>orders.status</c> as the lowercase code from <see cref="OrderStatusCodes"/>.
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
