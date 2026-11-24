using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Pricing;
using WebApplication1.Domain.Orders;
using WebApplication1.Options;

namespace WebApplication1.Services.Orders;

public sealed class PricingService : IPricingService
{
    internal const string FirstOrderCouponError = "โค้ดนี้ใช้ได้เฉพาะบัญชีที่ยังไม่มีคำสั่งซื้อที่ไม่ได้ถูกยกเลิก";

    internal static Task<bool> IsFirstOrderCouponUnavailableAsync(AppDbContext db, string code, int userId, CancellationToken token) =>
        string.Equals(code, "NEWUSER100", StringComparison.OrdinalIgnoreCase)
            ? db.Orders.AnyAsync(o => o.UserId == userId && o.Status != OrderStatus.Cancelled, token)
            : Task.FromResult(false);

    private readonly AppDbContext _db;
    private readonly CommerceOptions _commerce;
    private readonly TimeProvider _clock;

    public PricingService(AppDbContext db, IOptions<CommerceOptions> commerce, TimeProvider clock)
    {
        _db = db;
        _commerce = commerce.Value;
        _clock = clock;
    }

    public async Task<PricingQuote> QuoteAsync(IReadOnlyCollection<CartLineRequest> items, string? couponCode, int? userId, CancellationToken cancellationToken = default, bool useRewards = false)
    {
        var problems = new List<string>();
        var requested = items
            .Where(i => i.ProductId > 0)
            .GroupBy(i => i.ProductId)
            .Select(g => new CartLineRequest(g.Key, g.Sum(i => i.Quantity)))
            .ToList();

        var ids = requested.Select(r => r.ProductId).ToArray();
        var products = await _db.Products.AsNoTracking().Include(p => p.Shop).ThenInclude(p => p!.Shop)
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        var sales = await _db.ActiveFlashSales(_clock.GetUtcNow().UtcDateTime).AsNoTracking()
            .Where(s => ids.Contains(s.ProductId)).ToDictionaryAsync(s => s.ProductId, cancellationToken);

        var lines = new List<PricedLine>();
        foreach (var request in requested)
        {
            if (!products.TryGetValue(request.ProductId, out var product))
            {
                problems.Add("มีสินค้าในตะกร้าที่ไม่มีขายแล้ว กรุณาลบออกก่อน");
                continue;
            }

            if (product.Shop is null)
            {
                problems.Add($"{product.Name}: ยังไม่พร้อมจำหน่าย");
                continue;
            }

            if (request.Quantity < 1 || request.Quantity > _commerce.MaxQuantityPerItem)
            {
                problems.Add($"{product.Name}: สั่งได้ 1–{_commerce.MaxQuantityPerItem} ชิ้นต่อรายการ");
                continue;
            }

            if (product.Stock < request.Quantity)
            {
                problems.Add(product.Stock == 0
                    ? $"{product.Name} หมดแล้ว"
                    : $"{product.Name} เหลือเพียง {product.Stock} ชิ้น");
                continue;
            }

            var sale = sales.GetValueOrDefault(product.Id);
            if (sale is not null && (sale.Price >= product.Price || sale.Quota - sale.Sold < request.Quantity)) sale = null;
            lines.Add(new PricedLine(product.Id, product.Name, product.Category, product.ImageUrl, sale?.Price ?? product.Price,
                request.Quantity, product.Shop.ShopId, product.Shop.Shop.Name, product.Price, sale?.Id));
        }

        var subtotal = lines.Sum(l => l.LineTotal);
        var shippingBase = subtotal == 0 || subtotal >= _commerce.FreeShippingThreshold ? 0 : _commerce.ShippingFee;

        var (discount, shippingWaived, appliedCode, couponName, couponError, couponCategories) =
            await ApplyCouponAsync(couponCode, userId, lines, shippingBase, cancellationToken);
        var balance = userId is int customerId ? (await _db.LoyaltyBalanceAsync(customerId, cancellationToken)).Available : 0m;
        var reward = useRewards ? LoyaltyRules.Discount(balance, balance, subtotal, discount) : 0m;

        return new PricingQuote(lines, subtotal, discount, shippingBase - shippingWaived, shippingWaived,
            appliedCode, couponName, couponError, problems, balance, reward, couponCategories);
    }

    private async Task<(decimal Discount, decimal ShippingWaived, string? Code, string? Name, string? Error, IReadOnlyList<string>? Categories)> ApplyCouponAsync(
        string? couponCode, int? userId, IReadOnlyList<PricedLine> lines, decimal shippingFee, CancellationToken cancellationToken)
    {
        var code = couponCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
        {
            return (0, 0, null, null, null, null);
        }

        if (userId is null)
        {
            return (0, 0, null, null, "เข้าสู่ระบบก่อนใช้โค้ดส่วนลด", null);
        }

        var coupon = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
        if (coupon is null)
        {
            return (0, 0, null, null, $"ไม่พบโค้ด {code}", null);
        }

        var alreadyUsed = await _db.UserCoupons.AnyAsync(u => u.UserId == userId && u.CouponCode == code, cancellationToken);
        if (alreadyUsed)
        {
            return (0, 0, null, null, $"คุณใช้โค้ด {code} ไปแล้ว (ใช้ได้ 1 ครั้งต่อบัญชี)", null);
        }

        if (await IsFirstOrderCouponUnavailableAsync(_db, code, userId.Value, cancellationToken))
            return (0, 0, null, null, FirstOrderCouponError, null);

        var outcome = CouponCalculator.Evaluate(coupon, lines, _clock.GetUtcNow().UtcDateTime);
        if (!outcome.IsValid)
        {
            return (0, 0, null, null, outcome.Error, null);
        }

        return (outcome.Discount, outcome.FreeShipping ? shippingFee : 0, coupon.Code, coupon.Name, null, coupon.ApplicableCategoryList);
    }
}
