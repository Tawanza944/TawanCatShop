using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Backoffice;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Controllers;

/// <summary>จัดการสินค้าสำหรับบทบาท Shop โดยรับฟอร์มจากหลังร้านและพากลับด้วย Post/Redirect/Get</summary>
[RequireRole(Roles.Shop)]
public sealed class ShopController : Controller
{
    private readonly IProductService _products;
    private readonly ShopProfileService _profile;

    private readonly ShopInteractions _interactions;
    private readonly WebApplication1.Services.Identity.ICurrentUserService _user;

    public ShopController(IProductService products, ShopProfileService profile, ShopInteractions interactions,
        WebApplication1.Services.Identity.ICurrentUserService user)
    {
        _products = products;
        _profile = profile;
        _interactions = interactions;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Reviews(CancellationToken token) =>
        View(await _interactions.ReviewsAsync(_user.RequireUserId(), token));

    [HttpGet]
    public async Task<IActionResult> Messages(CancellationToken token) =>
        View(await _interactions.InboxAsync(_user.RequireUserId(), token));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyReview(int id, string? message, CancellationToken token)
    {
        var result = await _interactions.ReplyAsync(_user.RequireUserId(), id, message, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "บันทึกคำตอบแล้ว" : result.Error;
        return RedirectToAction(nameof(Reviews));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> SaveProfileImage(IFormFile? image, CancellationToken cancellationToken)
    {
        var result = await _profile.SaveImageAsync(image, cancellationToken);
        return Back(result.Error, result.Succeeded ? "บันทึกรูปโปรไฟล์ร้านแล้ว" : null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> SaveProduct(ProductForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Back(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "ข้อมูลสินค้าไม่ครบ");
        }

        var result = await _products.SaveAsync(form, cancellationToken);
        return Back(result.Error, result.Succeeded ? $"บันทึก {form.Name} แล้ว" : null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restock(int id, int amount, CancellationToken cancellationToken)
    {
        var result = await _products.RestockAsync(id, amount, cancellationToken);
        return Back(result.Error, result.Succeeded ? $"เติมสต็อก {amount} ชิ้น ตอนนี้มี {result.Value} ชิ้น" : null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var result = await _products.DeleteAsync(id, cancellationToken);
        return Back(result.Error, result.Succeeded ? "ลบสินค้าแล้ว" : null);
    }

    private IActionResult Back(string? error, string? success = null)
    {
        TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? success;
        return RedirectToAction(nameof(AccountController.Product), "Account");
    }
}
