namespace WebApplication1.Domain.Pricing;

/// <summary>What the browser asks for: a product and how many. Prices never come from the client.</summary>
public sealed record CartLineRequest(int ProductId, int Quantity);

/// <summary>A cart line priced from the database.</summary>
public sealed record PricedLine(int ProductId, string Name, string Category, string? ImageUrl, decimal UnitPrice, int Quantity, int ShopId = 0, string? ShopName = null,
    decimal? SellerPrice = null, int? FlashSaleId = null)
{
    public decimal LineTotal => UnitPrice * Quantity;
    public decimal SellerTotal => (SellerPrice ?? UnitPrice) * Quantity;
}

/// <summary>The server's price for a cart: what the customer will actually pay.</summary>
public sealed record PricingQuote(
    IReadOnlyList<PricedLine> Lines,
    decimal Subtotal,
    decimal Discount,
    decimal ShippingFee,
    decimal ShippingWaived,
    string? CouponCode,
    string? CouponName,
    string? CouponError,
    IReadOnlyList<string> Problems,
    decimal RewardBalance = 0,
    decimal RewardDiscount = 0,
    IReadOnlyList<string>? CouponCategories = null)
{
    public decimal Total => Math.Max(0, Subtotal - Discount - RewardDiscount + ShippingFee);

    public bool CanCheckout => Lines.Count > 0 && Problems.Count == 0;
}

/// <summary>Result of applying one coupon to a priced cart.</summary>
public sealed record CouponOutcome(decimal Discount, bool FreeShipping, string? Error)
{
    public bool IsValid => Error is null;

    public static CouponOutcome Rejected(string error) => new(0, false, error);
}
