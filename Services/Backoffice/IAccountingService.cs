using WebApplication1.Domain.Common;

namespace WebApplication1.Services.Backoffice;

/// <summary>
/// The ledger: income and costs computed from delivered orders (single source of truth) plus entries Accounting adds by hand.
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

    /// <summary>CSV (UTF-8 with BOM so Excel shows Thai correctly).</summary>
    byte[] ToCsv(LedgerReport report);
}

public sealed record PayoutQueueRow(int OrderId, string OrderNo, decimal NetAmount, bool Approved);
