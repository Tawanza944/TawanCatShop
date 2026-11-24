namespace WebApplication1.Models;

public sealed class MarketplaceShop
{
    public const string PlatformName = "LoveCat";
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
}

public sealed class ShopMembership
{
    public int UserId { get; set; }
    public int ShopId { get; set; }
}

public sealed class ShopProfileImage
{
    public int ShopId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public sealed class RiderProvince
{
    public int UserId { get; set; }
    public string ProvinceCode { get; set; } = string.Empty;
}

public sealed class ProductShop
{
    public int ProductId { get; set; }
    public int ShopId { get; set; }
    public MarketplaceShop Shop { get; set; } = null!;
}

public sealed class OrderShop
{
    public int OrderId { get; set; }
    public int ShopId { get; set; }
    public MarketplaceShop Shop { get; set; } = null!;
    public Guid? PaymentId { get; set; }
    public BasketPayment? Payment { get; set; }
    public string ProvinceCode { get; set; } = string.Empty;
    public bool RiderDelivery { get; set; }
}

public sealed class BasketPayment
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public decimal Total { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal ShippingFee { get; set; }
    public string? CouponCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = PaymentReview.Pending;
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Reason { get; set; }
    public BankSlip? Slip { get; set; }
}

public static class PaymentReview
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Rejected = "rejected";
    public const string Cancelled = "cancelled";
}

public sealed class BankSlip
{
    public Guid Id { get; set; }
    public Guid? PaymentId { get; set; }
    public int? PayoutOrderId { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
}
