using WebApplication1.Domain.Pricing;

namespace WebApplication1.Services.Orders;

/// <summary>คำนวณราคาตะกร้าจากฐานข้อมูล แล้วใช้กติกาคูปองและค่าส่ง</summary>
public interface IPricingService
{
    /// <param name="userId">รหัสลูกค้า ผู้ที่ยังไม่ล็อกอินใช้ null และจะใช้คูปองไม่ได้</param>
    Task<PricingQuote> QuoteAsync(IReadOnlyCollection<CartLineRequest> items, string? couponCode, int? userId, CancellationToken cancellationToken = default, bool useRewards = false);
}
