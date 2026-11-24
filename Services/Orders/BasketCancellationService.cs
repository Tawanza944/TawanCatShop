using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;

namespace WebApplication1.Services.Orders;

public sealed class BasketCancellationService(AppDbContext db, TimeProvider clock, IOrderWorkflow workflow)
{
    public async Task<OperationResult<bool>> CancelAsync(Guid paymentId, int actorId, string? reason, CancellationToken token)
    {
        reason = reason?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > 300)
            return OperationResult.Fail("ระบุเหตุผลที่ยกเลิกไม่เกิน 300 ตัวอักษร");

        var now = clock.GetUtcNow().UtcDateTime;
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        // เปลี่ยนสถานะยอดชำระก่อน เพื่อให้การตรวจสลิปและการยกเลิกแข่งกันได้เพียงผลเดียว
        var changed = await db.Set<BasketPayment>().Where(p => p.Id == paymentId &&
                (p.Status == PaymentReview.Pending || p.Status == PaymentReview.Rejected))
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.Status, PaymentReview.Cancelled)
                .SetProperty(p => p.ReviewedBy, actorId).SetProperty(p => p.ReviewedAt, now)
                .SetProperty(p => p.Reason, reason), token);
        if (changed != 1) return OperationResult.Fail("ยกเลิกได้เฉพาะยอดที่ยังไม่ยืนยันการชำระเงิน");

        var orders = db.Orders.Where(o => o.Marketplace!.PaymentId == paymentId);
        var batch = await orders.AsNoTracking().Include(o => o.Items).ToListAsync(token);
        if (batch.Count == 0 || batch.Any(o => o.Status != OrderStatus.Pending))
            return OperationResult.Fail("มีออเดอร์ที่ไม่ได้อยู่ในสถานะรอดำเนินการ");
        // ใช้รหัสออเดอร์อัปเดตตาราง orders โดยตรง เพื่อให้คำสั่งทำงานบน MySQL
        var orderIds = batch.Select(o => o.Id).ToArray();
        var trackingNote = workflow.TrackingNoteFor(OrderStatus.Cancelled);
        var cancelled = await db.Orders.Where(o => orderIds.Contains(o.Id) && o.Status == OrderStatus.Pending)
            .ExecuteUpdateAsync(set => set.SetProperty(o => o.Status, OrderStatus.Cancelled)
                .SetProperty(o => o.TrackingNote, trackingNote)
                .SetProperty(o => o.CancelledAt, now).SetProperty(o => o.UpdatedAt, now), token);
        if (cancelled != batch.Count) return OperationResult.Fail("สถานะออเดอร์เปลี่ยนแล้ว กรุณาโหลดหน้าใหม่");

        var items = batch.SelectMany(o => o.Items).ToList();
        var itemIds = items.Select(i => i.Id).ToList();
        var reservations = await db.Set<FlashReservation>().AsNoTracking().Where(r => itemIds.Contains(r.OrderItemId))
            .ToDictionaryAsync(r => r.OrderItemId, r => r.FlashSaleId, token);
        foreach (var product in items.GroupBy(i => i.ProductId).OrderBy(g => g.Key))
        {
            var quantity = product.Sum(i => i.Quantity);
            await db.Products.Where(p => p.Id == product.Key)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock + quantity).SetProperty(p => p.UpdatedAt, now), token);
            foreach (var sale in product.Where(i => reservations.ContainsKey(i.Id)).GroupBy(i => reservations[i.Id]))
            {
                var reserved = sale.Sum(i => i.Quantity);
                await db.Set<FlashSale>().Where(s => s.Id == sale.Key && s.Sold >= reserved)
                    .ExecuteUpdateAsync(set => set.SetProperty(s => s.Sold, s => s.Sold - reserved), token);
            }
        }

        var orderNumbers = batch.Select(o => o.OrderNo).ToList();
        var customerId = batch[0].UserId;
        await db.UserCoupons.Where(c => c.UserId == customerId && c.OrderNo != null && orderNumbers.Contains(c.OrderNo))
            .ExecuteDeleteAsync(token);
        // ยอดเงินสะสมไม่นับออเดอร์ที่ยกเลิก เครดิตที่ใช้จึงกลับมาเมื่อธุรกรรมบันทึกสำเร็จ
        await transaction.CommitAsync(token);
        return OperationResult.Ok();
    }
}
