using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Filters;
using WebApplication1.Models;
using WebApplication1.Services.Identity;

namespace WebApplication1.Controllers;

[RequireRole(Roles.Admin)]
public sealed class MarketplaceController(AppDbContext db, ICurrentUserService actor) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveShop(int id, string? name, string? pickupAddress, CancellationToken token)
    {
        name = name?.Trim();
        pickupAddress = pickupAddress?.Trim();
        if (string.Equals(name, MarketplaceShop.PlatformName, StringComparison.OrdinalIgnoreCase) ||
            await db.Set<MarketplaceShop>().AnyAsync(shop => shop.Id == id && shop.Name == MarketplaceShop.PlatformName, token))
            return Back("LoveCat เป็นชื่อแพลตฟอร์ม");
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100 ||
            string.IsNullOrWhiteSpace(pickupAddress) || pickupAddress.Length > 500)
            return Back("กรอกชื่อร้านและจุดรับสินค้าก่อนบันทึก");

        if (await db.Set<MarketplaceShop>().AnyAsync(shop => shop.Id != id && shop.Name == name, token))
            return Back("มีชื่อร้านนี้แล้ว");

        var shop = id == 0 ? new MarketplaceShop() : await db.Set<MarketplaceShop>().FindAsync([id], token);
        if (shop is null) return Back("ไม่พบร้านนี้");
        shop.Name = name;
        shop.PickupAddress = pickupAddress;
        if (id == 0) db.Add(shop);
        db.RecordAdminActivity(actor.UserName ?? "", $"บันทึกร้าน {name}");
        await db.SaveChangesAsync(token);
        return Back();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignShop(int userId, int shopId, CancellationToken token)
    {
        if (!await HasRole(userId, Roles.Shop, token) || !await db.Set<MarketplaceShop>().AnyAsync(shop => shop.Id == shopId && shop.Name != MarketplaceShop.PlatformName, token))
            return Back("เลือกร้านและบัญชีที่มีบทบาท Shop");

        var membership = await db.Set<ShopMembership>().FindAsync([userId], token);
        if (membership is null)
            db.Add(new ShopMembership { UserId = userId, ShopId = shopId });
        else
            membership.ShopId = shopId;
        db.RecordAdminActivity(actor.UserName ?? "", $"ผูกบัญชี {userId} กับร้าน {shopId}");
        await db.SaveChangesAsync(token);
        return Back();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignProvince(int userId, string? provinceCode, CancellationToken token)
    {
        if (!DeliveryCoverage.UsesRider(provinceCode) || !await HasRole(userId, Roles.Raider, token))
            return Back("เลือกบัญชีไรเดอร์และจังหวัดที่ให้บริการ");

        var assignment = await db.Set<RiderProvince>().FindAsync([userId], token);
        if (assignment is null)
            db.Add(new RiderProvince { UserId = userId, ProvinceCode = provinceCode! });
        else
            assignment.ProvinceCode = provinceCode!;
        await db.SaveChangesAsync(token);
        return Back();
    }

    private Task<bool> HasRole(int userId, string role, CancellationToken token) =>
        db.Staff.AnyAsync(staff => staff.UserId == userId &&
            db.Roles.Any(value => value.Id == staff.RoleId && value.Name == role), token);

    private IActionResult Back(string? error = null)
    {
        TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "บันทึกแล้ว";
        return RedirectToAction(nameof(AccountController.Shops), "Account");
    }
}
