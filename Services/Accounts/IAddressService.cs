using WebApplication1.Domain.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.Accounts;

/// <summary>Saved delivery addresses of the signed-in user.</summary>
public interface IAddressService
{
    Task<IReadOnlyList<UserAddress>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Creates (id = 0) or updates an address that belongs to the user.</summary>
    Task<OperationResult<int>> SaveAsync(int userId, UserAddress address, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DeleteAsync(int userId, int addressId, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> SetDefaultAsync(int userId, int addressId, CancellationToken cancellationToken = default);
}
