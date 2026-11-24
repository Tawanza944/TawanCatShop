using WebApplication1.Domain.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.Accounts;

/// <summary>จัดการที่อยู่จัดส่งที่บันทึกไว้ของผู้ใช้ที่ล็อกอิน</summary>
public interface IAddressService
{
    Task<IReadOnlyList<UserAddress>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>เพิ่มที่อยู่เมื่อ id เท่ากับ 0 หรือแก้รายการที่ผู้ใช้เป็นเจ้าของ</summary>
    Task<OperationResult<int>> SaveAsync(int userId, UserAddress address, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DeleteAsync(int userId, int addressId, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> SetDefaultAsync(int userId, int addressId, CancellationToken cancellationToken = default);
}
