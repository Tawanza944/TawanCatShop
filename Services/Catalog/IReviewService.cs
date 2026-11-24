using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Store;

namespace WebApplication1.Services.Catalog;

/// <summary>ลูกค้าที่ได้รับสินค้าแล้วรีวิวได้หนึ่งครั้งต่อสินค้า และแก้รีวิวของตัวเองได้</summary>
public interface IReviewService
{
    Task<IReadOnlyList<ReviewItem>> GetForProductAsync(int productId, int? viewerUserId, CancellationToken cancellationToken = default);

    Task<ReviewEligibility> GetEligibilityAsync(int productId, int? userId, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> SaveAsync(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default);

    /// <summary>รหัสสินค้าที่ส่งถึงผู้ใช้แล้วและยังไม่มีรีวิวจากผู้ใช้รายนั้น</summary>
    Task<IReadOnlySet<int>> GetReviewableProductIdsAsync(int userId, CancellationToken cancellationToken = default);
}
