using WebApplication1.Domain.Pricing;

namespace WebApplication1.Services.Orders;

/// <summary>Prices a cart from database prices, applies a coupon and the shipping rule.</summary>
public interface IPricingService
{
    /// <param name="userId">The shopper, or null for a guest (guests cannot use coupons).</param>
    Task<PricingQuote> QuoteAsync(IReadOnlyCollection<CartLineRequest> items, string? couponCode, int? userId, CancellationToken cancellationToken = default, bool useRewards = false);
}
