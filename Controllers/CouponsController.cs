using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Accounts;
using WebApplication1.Services.Identity;

namespace WebApplication1.Controllers;

[RequireRole(Roles.User)]
public sealed class CouponsController(CouponCollection coupons, ICurrentUserService user) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int couponId, CancellationToken token)
    {
        var result = await coupons.SaveAsync(user.RequireUserId(), couponId, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "เก็บไว้ในคูปองของฉันแล้ว" : result.Error;
        return RedirectToAction(nameof(HomeController.Promotion), "Home");
    }
}
