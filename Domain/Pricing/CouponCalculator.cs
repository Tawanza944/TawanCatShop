using WebApplication1.Models;

namespace WebApplication1.Domain.Pricing;

/// <summary>
/// คำนวณส่วนลดจากกติกาคูปองและราคาตะกร้าโดยไม่อ่านฐานข้อมูล
/// หน้าดูยอดและการสั่งซื้อใช้กฎเดียวกันเพื่อให้ผลคำนวณตรงกัน
/// </summary>
public static class CouponCalculator
{
    public static CouponOutcome Evaluate(Coupon coupon, IReadOnlyList<PricedLine> lines, DateTime nowUtc)
    {
        if (!coupon.IsActive)
        {
            return CouponOutcome.Rejected($"โค้ด {coupon.Code} ปิดใช้งานแล้ว");
        }

        if (coupon.ExpiresAt is { } expiresAt && expiresAt < nowUtc)
        {
            return CouponOutcome.Rejected($"โค้ด {coupon.Code} หมดอายุแล้ว");
        }

        var subtotal = lines.Sum(l => l.LineTotal);
        if (coupon.MinSpend > 0 && subtotal < coupon.MinSpend)
        {
            return CouponOutcome.Rejected($"โค้ดนี้ใช้ได้เมื่อซื้อครบ {coupon.MinSpend:N0} บาท (ตอนนี้ {subtotal:N0} บาท)");
        }

        var categories = coupon.ApplicableCategoryList;
        var eligibleSubtotal = categories.Count == 0
            ? subtotal
            : lines.Where(l => categories.Contains(l.Category)).Sum(l => l.LineTotal);

        if (eligibleSubtotal <= 0)
        {
            return CouponOutcome.Rejected($"โค้ดนี้ใช้ได้เฉพาะ {string.Join(", ", categories)}");
        }

        return coupon.DiscountType switch
        {
            CouponTypes.FreeShipping => new CouponOutcome(0, true, null),
            CouponTypes.Percent => new CouponOutcome(Cap(Math.Round(eligibleSubtotal * coupon.DiscountValue / 100m, 2), coupon.MaxDiscount, eligibleSubtotal), false, null),
            CouponTypes.Fixed => new CouponOutcome(Math.Min(coupon.DiscountValue, eligibleSubtotal), false, null),
            _ => CouponOutcome.Rejected("โค้ดนี้ตั้งค่าไม่ถูกต้อง")
        };
    }

    private static decimal Cap(decimal discount, decimal? maxDiscount, decimal eligibleSubtotal)
    {
        var capped = maxDiscount is > 0 ? Math.Min(discount, maxDiscount.Value) : discount;
        return Math.Min(capped, eligibleSubtotal);
    }
}
