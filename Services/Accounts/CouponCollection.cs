using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.Accounts;

public sealed class CouponCollection(AppDbContext db, TimeProvider clock)
{
    public async Task<OperationResult<bool>> SaveAsync(int userId, int couponId, CancellationToken token = default)
    {
        var rows = db.Set<SavedCoupon>().Where(row => row.UserId == userId && row.CouponId == couponId);
        if (await rows.AnyAsync(token)) return OperationResult.Ok();

        var now = clock.GetUtcNow().UtcDateTime;
        var usable = await db.Coupons.AnyAsync(coupon => coupon.Id == couponId && coupon.IsActive &&
            (coupon.ExpiresAt == null || coupon.ExpiresAt >= now) &&
            !db.UserCoupons.Any(used => used.UserId == userId && used.CouponCode == coupon.Code), token);
        if (!usable) return OperationResult.Fail("คูปองนี้เก็บไม่ได้แล้ว");

        var saved = new SavedCoupon { UserId = userId, CouponId = couponId, SavedAt = now };
        db.Add(saved);
        try
        {
            await db.SaveChangesAsync(token);
        }
        catch (DbUpdateException)
        {
            db.Entry(saved).State = EntityState.Detached;
            if (!await rows.AnyAsync(token)) throw;
        }
        return OperationResult.Ok();
    }
}
