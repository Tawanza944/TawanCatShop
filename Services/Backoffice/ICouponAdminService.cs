using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Services.Backoffice;

public interface ICouponAdminService
{
    Task<OperationResult<int>> SaveAsync(CouponForm form, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> ToggleAsync(int couponId, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DeleteAsync(int couponId, CancellationToken cancellationToken = default);
}
