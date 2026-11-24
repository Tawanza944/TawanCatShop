using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Account;

namespace WebApplication1.Services.Accounts;

/// <summary>จัดการข้อมูลส่วนตัว รูปโปรไฟล์ และรหัสผ่านของบัญชีที่ล็อกอิน</summary>
public interface IProfileService
{
    Task<OperationResult<bool>> UpdateDetailsAsync(int userId, ProfileForm form, CancellationToken cancellationToken = default);

    Task<OperationResult<string>> UpdateAvatarAsync(int userId, IFormFile file, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> ChangePasswordAsync(int userId, PasswordChangeForm form, CancellationToken cancellationToken = default);
}
