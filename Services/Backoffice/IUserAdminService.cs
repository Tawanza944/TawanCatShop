using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Services.Backoffice;

/// <summary>บริการจัดการบัญชีสำหรับแอดมิน พร้อมบันทึกการเปลี่ยนแปลงใน admin_activity</summary>
public interface IUserAdminService
{
    Task<OperationResult<int>> SaveAsync(UserForm form, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DemoteAdminAsync(int userId, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> SetSuspendedAsync(int userId, bool suspended, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DeleteAsync(int userId, CancellationToken cancellationToken = default);
}
