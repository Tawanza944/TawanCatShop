using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.Backoffice;

public sealed record SellerReview(int Id, string Product, byte Rating, string? Comment, string? Reply);
public sealed record ShopConversation(int ShopId, string ShopName, int CustomerId, bool IsSeller, IReadOnlyList<ShopMessage> Messages);

public sealed class ShopInteractions(AppDbContext db, TimeProvider clock)
{
    private IQueryable<ProductReview> SellerReviews(int sellerId) =>
        db.ProductReviews.Where(r => db.ProductsFor(sellerId).Any(p => p.Id == r.ProductId));

    public async Task<IReadOnlyList<SellerReview>> ReviewsAsync(int sellerId, CancellationToken token) =>
        await (from review in SellerReviews(sellerId)
               join product in db.Products on review.ProductId equals product.Id
               orderby review.CreatedAt descending
               select new SellerReview(review.Id, product.Name, review.Rating, review.Comment,
                   db.Set<ShopReviewReply>().Where(r => r.ReviewId == review.Id && r.ShopId == product.Shop!.ShopId)
                       .Select(r => r.Message).FirstOrDefault())).Take(100).ToListAsync(token);

    public async Task<OperationResult<bool>> ReplyAsync(int sellerId, int reviewId, string? message, CancellationToken token)
    {
        message = message?.Trim();
        if (message is null || message.Length is < 1 or > 1000) return OperationResult.Fail("กรอกคำตอบ 1–1,000 ตัวอักษร");
        var shopId = await db.ShopIdAsync(sellerId, token);
        if (shopId is null || !await SellerReviews(sellerId).AnyAsync(r => r.Id == reviewId, token))
            return OperationResult.Fail("ไม่พบรีวิวของร้านนี้");

        var replies = db.Set<ShopReviewReply>();
        var reply = await replies.FindAsync([reviewId], token);
        if (reply is null)
        {
            reply = new ShopReviewReply { ReviewId = reviewId };
            replies.Add(reply);
        }
        reply.ShopId = shopId.Value;
        reply.Message = message;
        try
        {
            await db.SaveChangesAsync(token);
        }
        catch (DbUpdateException error) when (error.InnerException is MySql.Data.MySqlClient.MySqlException { Number: 1062 })
        {
            return OperationResult.Fail("มีการบันทึกคำตอบพร้อมกัน กรุณาเปิดรีวิวแล้วลองอีกครั้ง");
        }
        return OperationResult.Ok();
    }

    public async Task SetFollowAsync(int userId, int shopId, bool follow, CancellationToken token)
    {
        var follows = db.Set<ShopFollow>();
        if (!follow)
        {
            await follows.Where(f => f.UserId == userId && f.ShopId == shopId).ExecuteDeleteAsync(token);
            return;
        }
        if (!await db.Set<MarketplaceShop>().AnyAsync(s => s.Id == shopId, token)
            || await follows.AnyAsync(f => f.UserId == userId && f.ShopId == shopId, token)) return;
        follows.Add(new ShopFollow { UserId = userId, ShopId = shopId });
        try
        {
            await db.SaveChangesAsync(token);
        }
        catch (DbUpdateException error) when (error.InnerException is MySql.Data.MySqlClient.MySqlException { Number: 1062 })
        {
            // รองรับกรณีคำขออื่นบันทึกการติดตามร้านเดียวกันให้ลูกค้าคนนี้ไปแล้ว
        }
    }

    public async Task<IReadOnlyList<int>> InboxAsync(int sellerId, CancellationToken token)
    {
        var shopId = await db.ShopIdAsync(sellerId, token);
        return await db.Set<ShopMessage>().Where(m => m.ShopId == shopId)
            .GroupBy(m => m.UserId).OrderByDescending(g => g.Max(m => m.Id))
            .Select(g => g.Key).ToListAsync(token);
    }

    private IQueryable<ShopMessage> Thread(int shopId, int customerId) =>
        db.Set<ShopMessage>().Where(m => m.ShopId == shopId && m.UserId == customerId);

    public async Task<ShopConversation?> ConversationAsync(int actorId, bool seller, int id, CancellationToken token)
    {
        var shopId = seller ? await db.ShopIdAsync(actorId, token) : id;
        var customerId = seller ? id : actorId;
        if (shopId is null) return null;
        var shopName = await db.Set<MarketplaceShop>().Where(s => s.Id == shopId).Select(s => s.Name).SingleOrDefaultAsync(token);
        if (shopName is null) return null;
        var messages = await Thread(shopId.Value, customerId).OrderByDescending(m => m.Id).Take(100).ToListAsync(token);
        if (seller && messages.Count == 0) return null;
        messages.Reverse();
        return new ShopConversation(shopId.Value, shopName, customerId, seller, messages);
    }

    public async Task<OperationResult<bool>> SendAsync(int actorId, bool seller, int id, string? message, CancellationToken token)
    {
        message = message?.Trim();
        if (message is null || message.Length is < 1 or > 1000) return OperationResult.Fail("กรอกข้อความ 1–1,000 ตัวอักษร");
        var conversation = await ConversationAsync(actorId, seller, id, token);
        if (conversation is null) return OperationResult.Fail("ไม่พบบทสนทนา");
        db.Add(new ShopMessage
        {
            ShopId = conversation.ShopId,
            UserId = conversation.CustomerId,
            FromShop = seller,
            Message = message,
            SentAt = clock.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync(token);
        return OperationResult.Ok();
    }
}
