using System.ComponentModel.DataAnnotations;
using WebApplication1.Domain.Pricing;
using WebApplication1.Domain.Addresses;

namespace WebApplication1.ViewModels.Checkout;

public sealed class CartLineInput
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; } = 1;

    public CartLineRequest ToRequest() => new(ProductId, Quantity);
}

/// <summary>Body of POST /Checkout/Quote — used to show the real price in the cart and checkout form.</summary>
public sealed class QuoteRequest
{
    public bool UseRewards { get; set; }
    public List<CartLineInput> Items { get; set; } = [];

    [StringLength(50)]
    public string? CouponCode { get; set; }
}

/// <summary>Body of POST /Checkout/PlaceOrder. Only product ids and quantities are trusted; prices come from the database.</summary>
public sealed class PlaceOrderRequest
{
    public bool UseRewards { get; set; }
    [Required(ErrorMessage = "กรอกชื่อผู้รับ")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "ชื่อผู้รับยาว 2–100 ตัวอักษร")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกเบอร์โทรผู้รับ")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "เบอร์มือถือต้องมี 10 หลักและขึ้นต้นด้วย 0")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกที่อยู่จัดส่ง")]
    [StringLength(500, MinimumLength = 15, ErrorMessage = "ที่อยู่ต้องยาวอย่างน้อย 15 ตัวอักษร")]
    public string ShippingAddress { get; set; } = string.Empty;

    public AddressRegion? Region { get; set; }

    public Guid RequestId { get; set; }

    public decimal ExpectedTotal { get; set; }

    public string? Slip { get; set; }

    [Required(ErrorMessage = "เลือกวิธีชำระเงิน")]
    public string PaymentMethod { get; set; } = string.Empty;

    [StringLength(50)]
    public string? CouponCode { get; set; }

    [MinLength(1, ErrorMessage = "ตะกร้าว่าง")]
    public List<CartLineInput> Items { get; set; } = [];
}
