using Microsoft.AspNetCore.Mvc;
using WebApplication1.Filters;
using WebApplication1.Domain.Identity;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Orders;
using WebApplication1.ViewModels.Checkout;

namespace WebApplication1.Controllers;

/// <summary>
/// รับคำขอ JSON จากตะกร้าและหน้าชำระเงิน ราคาสินค้า ส่วนลด และค่าส่งใช้ยอดที่เซิร์ฟเวอร์คำนวณ
/// </summary>
public sealed class CheckoutController : Controller
{
    private readonly IPricingService _pricing;
    private readonly ICheckoutService _checkout;
    private readonly ICurrentUserService _currentUser;

    public CheckoutController(IPricingService pricing, ICheckoutService checkout, ICurrentUserService currentUser)
    {
        _pricing = pricing;
        _checkout = checkout;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Quote([FromBody] QuoteRequest request, CancellationToken cancellationToken)
    {
        var quote = await _pricing.QuoteAsync(request.Items.Select(i => i.ToRequest()).ToList(), request.CouponCode, _currentUser.UserId, cancellationToken, request.UseRewards);

        return Json(new
        {
            success = true,
            lines = quote.Lines.Select(l => new { l.ProductId, l.Name, l.UnitPrice, l.Quantity, l.LineTotal, l.ShopId, l.ShopName, l.FlashSaleId }),
            quote.Subtotal,
            quote.Discount,
            quote.ShippingFee,
            quote.ShippingWaived,
            quote.RewardBalance,
            quote.RewardDiscount,
            quote.Total,
            quote.CouponCode,
            quote.CouponName,
            quote.CouponError,
            quote.Problems,
            quote.CanCheckout
        });
    }

    [HttpPost]
    [RequireRole(Roles.User)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrEmpty(m));
            return BadRequest(new { success = false, message = firstError ?? "ข้อมูลไม่ครบ" });
        }

        var result = await _checkout.PlaceOrderAsync(_currentUser.RequireUserId(), request, cancellationToken);
        if (!result.Succeeded)
        {
            return Conflict(new { success = false, message = result.Error });
        }

        return Json(new
        {
            success = true,
            orderNo = result.Value!.OrderNo,
            total = result.Value.Total,
            trackingUrl = Url.Action(nameof(AccountController.Tracking), "Account")
        });
    }
}
