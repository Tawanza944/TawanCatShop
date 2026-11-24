using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.ViewModels.Store;

namespace WebApplication1.Services.Catalog;

public sealed class ReviewService : IReviewService
{
    private const int MaxCommentLength = 1000;

    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public ReviewService(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ReviewItem>> GetForProductAsync(int productId, int? viewerUserId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Select(r => new { r.Id, r.UserId, Name = r.User!.Name, r.Rating, r.Comment, r.CreatedAt,
                Reply = _db.Set<ShopReviewReply>().Where(reply => reply.ReviewId == r.Id &&
                    _db.Set<ProductShop>().Any(shop => shop.ProductId == r.ProductId && shop.ShopId == reply.ShopId))
                    .Select(reply => reply.Message).FirstOrDefault() })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new ReviewItem(r.Id, MaskName(r.Name), r.Rating, r.Comment, r.CreatedAt, r.UserId == viewerUserId, r.Reply)).ToList();
    }

    public async Task<ReviewEligibility> GetEligibilityAsync(int productId, int? userId, CancellationToken cancellationToken = default)
    {
        if (userId is null)
        {
            return new ReviewEligibility(false, "เข้าสู่ระบบเพื่อเขียนรีวิว", null);
        }

        var existing = await _db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.UserId == userId)
            .Select(r => new ReviewItem(r.Id, "คุณ", r.Rating, r.Comment, r.CreatedAt, true))
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            return new ReviewEligibility(true, null, existing);
        }

        var received = await FindDeliveredOrderIdAsync(userId.Value, productId, cancellationToken);
        return received is null
            ? new ReviewEligibility(false, "รีวิวได้หลังจากได้รับสินค้านี้แล้ว", null)
            : new ReviewEligibility(true, null, null);
    }

    public async Task<OperationResult<bool>> SaveAsync(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default)
    {
        if (rating is < 1 or > 5)
        {
            return OperationResult.Fail("ให้คะแนน 1–5 ดาว");
        }

        var text = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (text?.Length > MaxCommentLength)
        {
            return OperationResult.Fail($"รีวิวยาวได้ไม่เกิน {MaxCommentLength} ตัวอักษร");
        }

        var now = _clock.GetUtcNow().UtcDateTime;
        var review = await _db.ProductReviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
        if (review is null)
        {
            var orderId = await FindDeliveredOrderIdAsync(userId, productId, cancellationToken);
            if (orderId is null)
            {
                return OperationResult.Fail("รีวิวได้หลังจากได้รับสินค้านี้แล้ว");
            }

            _db.ProductReviews.Add(new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                OrderId = orderId.Value,
                Rating = (byte)rating,
                Comment = text,
                CreatedAt = now
            });
        }
        else
        {
            review.Rating = (byte)rating;
            review.Comment = text;
            review.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await RefreshProductRatingAsync(productId, cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<IReadOnlySet<int>> GetReviewableProductIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var received = await _db.OrderItems.AsNoTracking()
            .Where(i => i.Order!.UserId == userId && i.Order.Status == OrderStatus.Delivered)
            .Select(i => i.ProductId).Distinct().ToListAsync(cancellationToken);
        var reviewed = await _db.ProductReviews.AsNoTracking()
            .Where(r => r.UserId == userId).Select(r => r.ProductId).ToListAsync(cancellationToken);
        return received.Except(reviewed).ToHashSet();
    }

    private Task<int?> FindDeliveredOrderIdAsync(int userId, int productId, CancellationToken cancellationToken) =>
        _db.OrderItems.AsNoTracking()
            .Where(i => i.ProductId == productId && i.Order!.UserId == userId && i.Order.Status == OrderStatus.Delivered)
            .OrderByDescending(i => i.OrderId)
            .Select(i => (int?)i.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>products.rating และ review_count เก็บยอดสรุปจาก product_reviews เพื่อให้โหลดหน้ารวมสินค้าได้เร็ว</summary>
    private async Task RefreshProductRatingAsync(int productId, CancellationToken cancellationToken)
    {
        var ratings = await _db.ProductReviews.Where(r => r.ProductId == productId).Select(r => (int)r.Rating).ToListAsync(cancellationToken);
        var average = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 1);
        await _db.Products.Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Rating, average).SetProperty(p => p.ReviewCount, ratings.Count), cancellationToken);
    }

    private static string MaskName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0) return "ลูกค้า";
        return trimmed.Length <= 3 ? trimmed[..1] + "**" : trimmed[..3] + "***";
    }
}
