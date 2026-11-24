namespace WebApplication1.Domain.Pricing;

/// <summary>สินค้ากับจำนวนที่เบราว์เซอร์ขอซื้อ เซิร์ฟเวอร์เป็นผู้กำหนดราคา</summary>
public sealed record CartLineRequest(int ProductId, int Quantity);

/// <summary>รายการในตะกร้าที่แทนราคาด้วยค่าจากฐานข้อมูลแล้ว</summary>
public sealed record PricedLine(int ProductId, string Name, string Category, string? ImageUrl, decimal UnitPrice, int Quantity, int ShopId = 0, string? ShopName = null,
    decimal? SellerPrice = null, int? FlashSaleId = null)
{
    public decimal LineTotal => UnitPrice * Quantity;
    public decimal SellerTotal => (SellerPrice ?? UnitPrice) * Quantity;
}

/// <summary>ผลคำนวณราคาตะกร้าฝั่งเซิร์ฟเวอร์ที่ใช้หายอดที่ลูกค้าต้องจ่าย</summary>
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

/// <summary>ผลจากการนำคูปองหนึ่งใบไปคำนวณกับตะกร้าที่คิดราคาแล้ว</summary>
public sealed record CouponOutcome(decimal Discount, bool FreeShipping, string? Error)
{
    public bool IsValid => Error is null;

    public static CouponOutcome Rejected(string error) => new(0, false, error);
}
