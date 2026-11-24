using WebApplication1.Domain.Common;

namespace WebApplication1.Services.Backoffice;

/// <summary>
/// รวมข้อมูลสมุดบัญชีที่คำนวณจากออเดอร์กับรายการที่ฝ่ายบัญชีบันทึกเอง
/// </summary>
public interface IAccountingService
{
    Task<LedgerReport> GetLedgerAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommissionRow>> GetAwaitingCommissionAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<decimal>> DeductCommissionAsync(int? orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PayoutQueueRow>> GetPayoutQueueAsync(CancellationToken cancellationToken);
    Task<OperationResult<bool>> ApprovePayoutAsync(int orderId, CancellationToken cancellationToken, bool deductCommission = false);
    Task<OperationResult<bool>> RecordTransferAsync(int orderId, DateOnly? date, string? reference, CancellationToken cancellationToken, WebApplication1.Models.BankSlip? slip = null);

    Task<OperationResult<bool>> AddEntryAsync(DateOnly date, string entryType, string title, decimal amount, string? note, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>ส่งออก CSV แบบ UTF-8 พร้อม BOM เพื่อให้ Excel อ่านภาษาไทยได้ถูกต้อง</summary>
    byte[] ToCsv(LedgerReport report);
}

public sealed record PayoutQueueRow(int OrderId, string OrderNo, decimal NetAmount, bool Approved);
