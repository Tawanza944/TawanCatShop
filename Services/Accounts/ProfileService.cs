using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Helpers;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Storage;
using WebApplication1.ViewModels.Account;

namespace WebApplication1.Services.Accounts;

public sealed class ProfileService : IProfileService
{
    private readonly AppDbContext _db;
    private readonly IImageStorage _images;
    private readonly IAuthSession _authSession;

    public ProfileService(AppDbContext db, IImageStorage images, IAuthSession authSession)
    {
        _db = db;
        _images = images;
        _authSession = authSession;
    }

    public async Task<OperationResult<bool>> UpdateDetailsAsync(int userId, ProfileForm form, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) return OperationResult.Fail("ไม่พบบัญชีนี้");

        var username = form.Username.Trim();
        var email = form.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Id != userId && u.Username == username, cancellationToken))
            return OperationResult.Fail($"ชื่อผู้ใช้ {username} มีคนใช้แล้ว");
        if (await _db.Users.AnyAsync(u => u.Id != userId && u.Email == email, cancellationToken))
            return OperationResult.Fail($"อีเมล {email} มีคนใช้แล้ว");

        user.Name = form.Name.Trim();
        user.Username = username;
        user.Email = email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(form.PhoneNumber) ? null : form.PhoneNumber.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        _authSession.RefreshProfile(user);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<string>> UpdateAvatarAsync(int userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) return OperationResult<string>.Failure("ไม่พบบัญชีนี้");

        var upload = await _images.SaveAsync(file, "avatars", cancellationToken);
        if (!upload.Succeeded) return upload;

        _images.Delete(user.AvatarUrl);
        user.AvatarUrl = upload.Value;
        await _db.SaveChangesAsync(cancellationToken);

        _authSession.SetAvatar(upload.Value!);
        return upload;
    }

    public async Task<OperationResult<bool>> ChangePasswordAsync(int userId, PasswordChangeForm form, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) return OperationResult.Fail("ไม่พบบัญชีนี้");

        // ตรวจรหัสผ่านผ่าน PasswordHelper ที่ระบบใช้ร่วมกัน
        if (!PasswordHelper.VerifyPassword(form.CurrentPassword, user.PasswordHash))
            return OperationResult.Fail("รหัสผ่านปัจจุบันไม่ถูกต้อง");
        if (form.CurrentPassword == form.NewPassword)
            return OperationResult.Fail("รหัสผ่านใหม่ต้องไม่ซ้ำกับรหัสเดิม");

        user.PasswordHash = PasswordHelper.HashPassword(form.NewPassword);
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }
}
