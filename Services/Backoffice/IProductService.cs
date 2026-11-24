using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Services.Backoffice;

/// <summary>Shop-side product management.</summary>
public interface IProductService
{
    Task<OperationResult<int>> SaveAsync(ProductForm form, CancellationToken cancellationToken = default);

    Task<OperationResult<int>> RestockAsync(int productId, int amount, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DeleteAsync(int productId, CancellationToken cancellationToken = default);
}
