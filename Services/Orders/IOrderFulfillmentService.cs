using WebApplication1.Domain.Common;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Services.Orders;

/// <summary>
/// Everything that happens to an order after checkout: Shop packs it, a Raider takes and delivers it,
/// or the customer/Shop cancels it. Acts as the signed-in user.
/// </summary>
public interface IOrderFulfillmentService
{
    Task<OperationResult<OrderStatus>> MoveToAsync(string orderNo, OrderStatus nextStatus, CancellationToken cancellationToken = default);

    /// <summary>Cancels the order, puts the stock back and gives the coupon right back to the customer.</summary>
    Task<OperationResult<bool>> CancelAsync(string orderNo, CancellationToken cancellationToken = default);
}
