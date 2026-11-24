using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Checkout;

namespace WebApplication1.Services.Orders;

public sealed record PlacedOrder(string OrderNo, decimal Total);

/// <summary>Turns a cart into an order: re-prices on the server, reserves stock, records the coupon, all in one transaction.</summary>
public interface ICheckoutService
{
    Task<OperationResult<PlacedOrder>> PlaceOrderAsync(int userId, PlaceOrderRequest request, CancellationToken cancellationToken = default);
}
