using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Store;

namespace WebApplication1.Services.Catalog;

/// <summary>Customer reviews. Only someone who received the product may review it, once, and may edit their own review.</summary>
public interface IReviewService
{
    Task<IReadOnlyList<ReviewItem>> GetForProductAsync(int productId, int? viewerUserId, CancellationToken cancellationToken = default);

    Task<ReviewEligibility> GetEligibilityAsync(int productId, int? userId, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> SaveAsync(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default);

    /// <summary>Product ids from the user's delivered orders that they have not reviewed yet.</summary>
    Task<IReadOnlySet<int>> GetReviewableProductIdsAsync(int userId, CancellationToken cancellationToken = default);
}
