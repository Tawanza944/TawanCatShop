using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Models;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Storage;

namespace WebApplication1.Services.Backoffice;

public sealed class ShopProfileService(AppDbContext db, ICurrentUserService user, IImageStorage images)
{
    public async Task<OperationResult<bool>> SaveImageAsync(IFormFile? file, CancellationToken token)
    {
        var shopId = await db.ShopIdAsync(user.RequireUserId(), token);
        if (shopId is null) return OperationResult.Fail("บัญชีนี้ยังไม่ได้ผูกกับร้านค้า");
        if (file is null || file.Length == 0) return OperationResult.Fail("เลือกรูปโปรไฟล์ร้านก่อนบันทึก");

        var profiles = db.Set<ShopProfileImage>();
        var profile = await profiles.SingleOrDefaultAsync(p => p.ShopId == shopId.Value, token);
        var previous = profile?.ImageUrl;
        var upload = await images.SaveAsync(file, "shops", token);
        if (!upload.Succeeded) return OperationResult.Fail(upload.Error!);

        if (profile is null)
        {
            profile = new ShopProfileImage { ShopId = shopId.Value };
            profiles.Add(profile);
        }

        profile.ImageUrl = upload.Value!;
        try
        {
            await db.SaveChangesAsync(token);
        }
        catch (DbUpdateException error) when (error is DbUpdateConcurrencyException ||
            error.InnerException is MySql.Data.MySqlClient.MySqlException { Number: 1062 })
        {
            images.Delete(upload.Value);
            return OperationResult.Fail("รูปโปรไฟล์ร้านเพิ่งถูกเปลี่ยน กรุณาลองอีกครั้ง");
        }

        images.Delete(previous);
        return OperationResult.Ok();
    }
}
