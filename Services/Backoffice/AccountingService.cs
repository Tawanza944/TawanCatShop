using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.Options;
using WebApplication1.Services.Identity;

namespace WebApplication1.Services.Backoffice;

public sealed class AccountingService : IAccountingService
{
    private readonly AppDbContext _db;
    private readonly CommerceOptions _commerce;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _clock;

    public AccountingService(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock)
    {
        _db = db;
        _commerce = commerce.Value;
        _currentUser = currentUser;
        _clock = clock;
        _zone = TimeZoneInfo.FindSystemTimeZoneById(_commerce.TimeZoneId);
    }

    private readonly TimeZoneInfo _zone;

    /// <summary>ใช้วันที่ของร้านตามเขตเวลา Asia/Bangkok เพราะช่วง 00:00 ถึงก่อน 07:00 วันที่ UTC ยังเป็นวันก่อนหน้า</summary>
    private DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), _zone).DateTime);

    /// <summary>แปลงเวลาเริ่มต้นวันของร้านเป็น UTC สำหรับค้นหาข้อมูลในฐาน</summary>
    private DateTime StartOfDayUtc(DateOnly day) => TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), _zone);

    public async Task<LedgerReport> GetLedgerAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var start = StartOfDayUtc(from);
        var end = StartOfDayUtc(to.AddDays(1));

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && (o.DeliveredAt ?? o.CreatedAt) >= start && (o.DeliveredAt ?? o.CreatedAt) < end)
            .Select(o => new { o.OrderNo, o.CustomerName, o.TotalAmount, IsBasket = o.Marketplace!.PaymentId != null, o.Subtotal, SnapshotCommission = (decimal?)o.ShopPayout!.Commission, o.CommissionDeducted, At = o.DeliveredAt ?? o.CreatedAt, o.RiderId })
            .ToListAsync(cancellationToken);

        var entries = await _db.LedgerEntries.AsNoTracking()
            .Where(e => e.EntryDate >= from && e.EntryDate <= to)
            .ToListAsync(cancellationToken);

        var lines = new List<LedgerLine>();
        foreach (var order in orders)
        {
            if (!order.IsBasket)
                lines.Add(new LedgerLine(order.At, order.OrderNo, "ลูกค้าชำระ", order.CustomerName, order.TotalAmount, 0, LedgerLineKind.CustomerPayment));
            if (order.RiderId.HasValue)
                lines.Add(new LedgerLine(order.At, order.OrderNo, "ค่าส่งไรเดอร์", "ไรเดอร์", 0, _commerce.RiderFeePerDelivery, LedgerLineKind.RiderFee));
        }

        var baskets = await _db.Set<BasketPayment>().AsNoTracking()
            .Where(p => p.Status == PaymentReview.Confirmed && p.ReviewedAt >= start && p.ReviewedAt < end)
            .Select(p => new { p.Id, p.ReviewedAt, p.Total, p.CouponDiscount, p.ShippingFee })
            .ToListAsync(cancellationToken);
        var basketCosts = await (from incentive in _db.Set<OrderIncentives>().AsNoTracking()
                                 join orderShop in _db.Set<OrderShop>() on incentive.OrderId equals orderShop.OrderId
                                 join payment in _db.Set<BasketPayment>() on orderShop.PaymentId equals (Guid?)payment.Id
                                 where payment.Status == PaymentReview.Confirmed && payment.ReviewedAt >= start && payment.ReviewedAt < end
                                 select new { payment.Id, incentive.FlashDiscount, incentive.RewardUsed }).ToListAsync(cancellationToken);
        var costsByBasket = basketCosts.ToLookup(cost => cost.Id);
        foreach (var basket in baskets)
        {
            var at = basket.ReviewedAt!.Value;
            var reference = basket.Id.ToString();
            var flash = costsByBasket[basket.Id].Sum(cost => cost.FlashDiscount);
            var rewards = costsByBasket[basket.Id].Sum(cost => cost.RewardUsed);
            lines.Add(new LedgerLine(at, reference, "ยอดสินค้าชำระรวม", null,
                basket.Total - basket.ShippingFee + basket.CouponDiscount + flash + rewards, 0, LedgerLineKind.CustomerPayment));
            if (basket.ShippingFee > 0)
                lines.Add(new LedgerLine(at, reference, "ค่าส่งที่ลูกค้าชำระ", null, basket.ShippingFee, 0, LedgerLineKind.ShippingCollected));
            if (basket.CouponDiscount > 0)
                lines.Add(new LedgerLine(at, reference, "ต้นทุนส่วนลดคูปอง", null, 0, basket.CouponDiscount, LedgerLineKind.CouponCost));
            if (flash > 0)
                lines.Add(new LedgerLine(at, reference, "ต้นทุน Flash Sale", null, 0, flash, LedgerLineKind.FlashSaleCost));
            if (rewards > 0)
                lines.Add(new LedgerLine(at, reference, "ต้นทุนเงินสะสม", null, 0, rewards, LedgerLineKind.RewardsCost));
        }

        var payouts = await _db.Set<ShopPayout>().AsNoTracking()
            .Where(p => p.TransferDate >= from && p.TransferDate <= to)
            .Select(p => new { p.TransferDate, p.Order.OrderNo, p.NetAmount, p.TransferReference })
            .ToListAsync(cancellationToken);
        foreach (var payout in payouts)
        {
            lines.Add(new LedgerLine(StartOfDayUtc(payout.TransferDate!.Value), payout.OrderNo,
                $"โอนให้ร้านค้า · {payout.TransferReference}", "ร้านค้า", 0, payout.NetAmount, LedgerLineKind.ShopPayout));
        }

        foreach (var entry in entries)
        {
            var income = entry.EntryType == LedgerEntry.Income;
            lines.Add(new LedgerLine(StartOfDayUtc(entry.EntryDate).AddHours(12), $"บันทึก #{entry.Id}", entry.Title, entry.Note,
                income ? entry.Amount : 0, income ? 0 : entry.Amount, income ? LedgerLineKind.ManualIncome : LedgerLineKind.ManualExpense, entry.Id));
        }

        // คำนวณยอดสะสมจากเก่าไปใหม่ แล้วกลับลำดับเพื่อแสดงรายการล่าสุดก่อน
        decimal balance = 0;
        var ordered = lines.OrderBy(l => l.At).ThenBy(l => l.Kind).Select(l =>
        {
            balance += l.MoneyIn - l.MoneyOut;
            return l with { Balance = balance };
        }).Reverse().ToList();

        var outstanding = await _db.Orders.AsNoTracking().Where(o => o.Status == OrderStatus.Delivered &&
                (o.DeliveredAt ?? o.CreatedAt) < end && (o.ShopPayout == null || o.ShopPayout.TransferDate == null || o.ShopPayout.TransferDate > to))
            .Select(o => new { o.Subtotal, Net = (decimal?)o.ShopPayout!.NetAmount }).ToListAsync(cancellationToken);
        decimal Expense(LedgerLineKind kind) => lines.Where(l => l.Kind == kind).Sum(l => l.MoneyOut);
        var summary = new LedgerSummary(
            lines.Sum(l => l.MoneyIn),
            lines.Sum(l => l.MoneyOut),
            lines.Where(l => l.Kind == LedgerLineKind.ShopPayout).Sum(l => l.MoneyOut),
            lines.Where(l => l.Kind == LedgerLineKind.RiderFee).Sum(l => l.MoneyOut),
            orders.Where(o => o.CommissionDeducted).Sum(o => o.SnapshotCommission ?? PayoutRules.Commission(o.Subtotal, _commerce.CommissionRate)),
            orders.Count(o => !o.CommissionDeducted),
            Expense(LedgerLineKind.CouponCost), Expense(LedgerLineKind.FlashSaleCost), Expense(LedgerLineKind.RewardsCost),
            Expense(LedgerLineKind.ManualExpense), lines.Where(l => l.Kind == LedgerLineKind.ShippingCollected).Sum(l => l.MoneyIn),
            outstanding.Sum(o => o.Net ?? o.Subtotal - PayoutRules.Commission(o.Subtotal, _commerce.CommissionRate)));

        return new LedgerReport(from, to, ordered, summary);
    }

    public async Task<IReadOnlyList<CommissionRow>> GetAwaitingCommissionAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && !o.CommissionDeducted)
            .OrderBy(o => o.DeliveredAt ?? o.CreatedAt)
            .Select(o => new { o.Id, o.OrderNo, o.CustomerName, At = o.DeliveredAt ?? o.CreatedAt, o.Subtotal })
            .ToListAsync(cancellationToken);

        return rows.Select(o =>
        {
            var commission = PayoutRules.Commission(o.Subtotal, _commerce.CommissionRate);
            return new CommissionRow(o.Id, o.OrderNo, o.CustomerName, o.At, o.Subtotal, commission, o.Subtotal - commission);
        }).ToList();
    }

    public async Task<OperationResult<decimal>> DeductCommissionAsync(int? orderId, CancellationToken cancellationToken = default)
    {
        var query = _db.Orders.Where(o => o.Status == OrderStatus.Delivered && !o.CommissionDeducted);
        if (orderId is int id)
        {
            query = query.Where(o => o.Id == id);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var candidates = await query.OrderBy(o => o.Id).Select(o => new { o.Id, o.Subtotal }).ToListAsync(cancellationToken);
        var totalCommission = 0m;
        var updated = 0;
        foreach (var candidate in candidates)
        {
            var changed = await query.Where(o => o.Id == candidate.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.CommissionDeducted, true), cancellationToken);
            if (changed == 1)
            {
                totalCommission += PayoutRules.Commission(candidate.Subtotal, _commerce.CommissionRate);
                updated++;
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return updated == 0
            ? OperationResult<decimal>.Failure("ไม่มีรายการที่ต้องหักคอม (ต้องเป็นออเดอร์ที่ส่งถึงแล้ว)")
            : OperationResult<decimal>.Success(totalCommission);
    }

    public async Task<IReadOnlyList<PayoutQueueRow>> GetPayoutQueueAsync(CancellationToken cancellationToken)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && o.CommissionDeducted &&
                (o.ShopPayout == null || o.ShopPayout.TransferDate == null))
            .OrderBy(o => o.DeliveredAt ?? o.CreatedAt).ThenBy(o => o.Id)
            .Select(o => new { o.Id, o.OrderNo, o.Subtotal, Net = (decimal?)o.ShopPayout!.NetAmount })
            .ToListAsync(cancellationToken);

        return orders.Select(o => new PayoutQueueRow(o.Id, o.OrderNo,
            o.Net ?? (o.Subtotal - PayoutRules.Commission(o.Subtotal, _commerce.CommissionRate)), o.Net.HasValue)).ToList();
    }

    public async Task<OperationResult<bool>> ApprovePayoutAsync(int orderId, CancellationToken cancellationToken, bool deductCommission = false)
    {
        var order = await _db.Orders.AsNoTracking().Where(o => o.Id == orderId)
            .Select(o => new { o.Status, o.CommissionDeducted, o.Subtotal, HasPayout = o.ShopPayout != null })
            .SingleOrDefaultAsync(cancellationToken);
        if (order is null || !PayoutRules.CanApprove(order.Status, order.CommissionDeducted || deductCommission, order.HasPayout))
            return OperationResult.Fail(deductCommission
                ? "อนุมัติได้เฉพาะออเดอร์ที่ส่งถึงแล้วและยังไม่เคยอนุมัติ"
                : "อนุมัติได้เฉพาะออเดอร์ที่ส่งถึงแล้ว หักคอมแล้ว และยังไม่เคยอนุมัติ");

        var commission = PayoutRules.Commission(order.Subtotal, _commerce.CommissionRate);
        var payout = new ShopPayout
        {
            OrderId = orderId,
            Commission = commission,
            NetAmount = order.Subtotal - commission,
            ApprovedBy = _currentUser.RequireUserId(),
            ApprovedAt = _clock.GetUtcNow().UtcDateTime
        };
        // บันทึกสถานะหักค่าคอมและยอดโอนพร้อมกัน เพื่อไม่ให้ข้อมูลสองส่วนคลาดกัน
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!order.CommissionDeducted)
            {
                await _db.Orders.Where(o => o.Id == orderId && o.Status == OrderStatus.Delivered && !o.CommissionDeducted)
                    .ExecuteUpdateAsync(update => update.SetProperty(o => o.CommissionDeducted, true), cancellationToken);
            }

            _db.Add(payout);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _db.Entry(payout).State = EntityState.Detached;
            if (await _db.Set<ShopPayout>().AnyAsync(p => p.OrderId == orderId, cancellationToken))
                return OperationResult.Fail("ออเดอร์นี้อนุมัติแล้ว");
            throw;
        }

        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> RecordTransferAsync(int orderId, DateOnly? date, string? reference, CancellationToken cancellationToken, BankSlip? slip = null)
    {
        if (!PayoutRules.IsValidTransfer(date, Today, reference))
            return OperationResult.Fail("กรอกวันที่โอนที่ไม่เกินวันนี้และเลขอ้างอิงไม่เกิน 100 ตัวอักษร");

        var actor = _currentUser.RequireUserId();
        var now = _clock.GetUtcNow().UtcDateTime;
        if (slip is not null && await _db.Set<BankSlip>().AnyAsync(saved => saved.Checksum == slip.Checksum, cancellationToken))
            return OperationResult.Fail("สลิปนี้ถูกบันทึกไว้แล้ว");

        var transferReference = reference!.Trim();
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var changed = await _db.Set<ShopPayout>()
            .Where(p => p.OrderId == orderId && p.TransferDate == null &&
                _db.Orders.Any(o => o.Id == p.OrderId && o.Status == OrderStatus.Delivered && o.CommissionDeducted))
            .ExecuteUpdateAsync(update => update
                .SetProperty(p => p.TransferDate, date)
                .SetProperty(p => p.TransferReference, transferReference)
                .SetProperty(p => p.RecordedBy, actor)
                .SetProperty(p => p.RecordedAt, now), cancellationToken);

        if (changed != 1)
            return OperationResult.Fail("ต้องอนุมัติเงินก่อนบันทึกการโอน และบันทึกได้ครั้งเดียว");

        if (slip is not null)
        {
            slip.PayoutOrderId = orderId;
            _db.Add(slip);
            await _db.SaveChangesAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> AddEntryAsync(DateOnly date, string entryType, string title, decimal amount, string? note, CancellationToken cancellationToken = default)
    {
        if (entryType is not (LedgerEntry.Income or LedgerEntry.Expense)) return OperationResult.Fail("เลือกประเภทรายการ");
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200) return OperationResult.Fail("กรอกชื่อรายการ (ไม่เกิน 200 ตัว)");
        if (amount is <= 0 or > 10_000_000) return OperationResult.Fail("จำนวนเงินต้องมากกว่า 0");
        if (date > Today) return OperationResult.Fail("บันทึกล่วงหน้าไม่ได้");

        _db.LedgerEntries.Add(new LedgerEntry
        {
            EntryDate = date,
            EntryType = entryType,
            Title = title.Trim(),
            Amount = Math.Round(amount, 2),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedBy = _currentUser.RequireUserId(),
            CreatedAt = _clock.GetUtcNow().UtcDateTime
        });
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var deleted = await _db.LedgerEntries.Where(e => e.Id == entryId).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? OperationResult.Fail("ไม่พบรายการนี้") : OperationResult.Ok();
    }

    public byte[] ToCsv(LedgerReport report)
    {
        var csv = new StringBuilder();
        csv.AppendLine("วันที่,อ้างอิง,รายการ,คู่ค้า,เงินเข้า,เงินออก,คงเหลือ");
        foreach (var line in report.Lines.Reverse())
        {
            csv.AppendLine(string.Join(',',
                line.At.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Escape(line.Reference), Escape(line.Title), Escape(line.Party),
                line.MoneyIn.ToString("0.00", CultureInfo.InvariantCulture),
                line.MoneyOut.ToString("0.00", CultureInfo.InvariantCulture),
                line.Balance.ToString("0.00", CultureInfo.InvariantCulture)));
        }

        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
    }

    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        // ป้องกันข้อความถูกตีความเป็นสูตรในสเปรดชีต และครอบค่าที่มีตัวคั่นตามรูปแบบ CSV
        if (text.Length > 0 && "=+-@".Contains(text[0])) text = "'" + text;
        return text.Contains(',') || text.Contains('"') || text.Contains('\n') ? $"\"{text.Replace("\"", "\"\"")}\"" : text;
    }
}
