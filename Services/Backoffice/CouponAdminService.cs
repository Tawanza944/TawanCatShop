using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Pricing;
using WebApplication1.Models;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Services.Backoffice;

public sealed class CouponAdminService : ICouponAdminService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CouponAdminService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private string Actor => _currentUser.UserName ?? string.Empty;

    public async Task<OperationResult<int>> SaveAsync(CouponForm form, CancellationToken cancellationToken = default)
    {
        if (!CouponTypes.All.Contains(form.DiscountType))
        {
            return OperationResult<int>.Failure("ชนิดส่วนลดไม่ถูกต้อง");
        }

        if (form.DiscountType == CouponTypes.Percent && form.DiscountValue is <= 0 or > 90)
        {
            return OperationResult<int>.Failure("ส่วนลดเปอร์เซ็นต์ต้องอยู่ระหว่าง 1–90");
        }

        if (form.DiscountType == CouponTypes.Fixed && form.DiscountValue <= 0)
        {
            return OperationResult<int>.Failure("ส่วนลดต้องมากกว่า 0 บาท");
        }

        var categories = form.Categories.Where(c => StorefrontCategories.FindByName(c) is not null).Distinct().ToList();
        var code = form.Code.Trim().ToUpperInvariant();
        if (await _db.Coupons.AnyAsync(c => c.Id != form.Id && c.Code == code, cancellationToken))
        {
            return OperationResult<int>.Failure($"มีโค้ด {code} อยู่แล้ว");
        }

        Coupon coupon;
        if (form.Id == 0)
        {
            coupon = new Coupon { CreatedAt = DateTime.UtcNow };
            _db.Coupons.Add(coupon);
        }
        else
        {
            var existing = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == form.Id, cancellationToken);
            if (existing is null)
            {
                return OperationResult<int>.Failure("ไม่พบโค้ดนี้");
            }

            coupon = existing;
        }

        coupon.Code = code;
        coupon.Name = form.Name.Trim();
        coupon.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
        coupon.DiscountType = form.DiscountType;
        coupon.DiscountValue = form.DiscountType == CouponTypes.FreeShipping ? 0 : form.DiscountValue;
        coupon.MinSpend = form.MinSpend;
        coupon.MaxDiscount = form.DiscountType == CouponTypes.Percent && form.MaxDiscount is > 0 ? form.MaxDiscount : null;
        coupon.ApplicableCategories = categories.Count == 0 ? null : string.Join(',', categories);
        coupon.ExpiresAt = form.ExpiresAt?.ToUniversalTime();
        coupon.IsActive = form.IsActive;

        _db.RecordAdminActivity(Actor, $"{(form.Id == 0 ? "เพิ่ม" : "แก้ไข")}คูปอง {code}");
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult<int>.Success(coupon.Id);
    }

    public async Task<OperationResult<bool>> ToggleAsync(int couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
        if (coupon is null)
        {
            return OperationResult.Fail("ไม่พบโค้ดนี้");
        }

        coupon.IsActive = !coupon.IsActive;
        _db.RecordAdminActivity(Actor, $"{(coupon.IsActive ? "เปิด" : "ปิด")}คูปอง {coupon.Code}");
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> DeleteAsync(int couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
        if (coupon is null)
        {
            return OperationResult.Fail("ไม่พบโค้ดนี้");
        }

        _db.Coupons.Remove(coupon);
        _db.RecordAdminActivity(Actor, $"ลบคูปอง {coupon.Code}");
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }
}
