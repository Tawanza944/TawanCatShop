namespace WebApplication1.Domain.Orders;

/// <summary>
/// State machine for orders: which role may move an order from one status to another.
/// </summary>
public interface IOrderWorkflow
{
    bool CanTransition(OrderStatus from, OrderStatus to, string? actorRole);

    /// <summary>Message shown to the customer on the tracking page after the order reaches <paramref name="status"/>.</summary>
    string TrackingNoteFor(OrderStatus status);
}
