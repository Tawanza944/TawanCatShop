using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Backoffice;
using WebApplication1.Services.Orders;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Controllers;

[RequireRole(Roles.Accounting)]
public sealed class AccountingController : Controller
{
    private readonly IAccountingService _accounting;

    public AccountingController(IAccountingService accounting)
    {
        _accounting = accounting;
    }

    /// <param name="orderId">One order, or null to deduct every waiting order.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeductCommission(int? orderId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "รหัสออเดอร์ไม่ถูกต้อง";
            return RedirectToAction(nameof(AccountController.Commission), "Account");
        }

        var result = await _accounting.DeductCommissionAsync(orderId, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? $"หักค่าคอมแล้ว รวม ฿{result.Value:N2}" : result.Error;
        return RedirectToAction(nameof(AccountController.Commission), "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePayout(int orderId, CancellationToken cancellationToken)
    {
        var result = await _accounting.ApprovePayoutAsync(orderId, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "อนุมัติแล้ว รอโอน" : result.Error;
        return RedirectToAction(nameof(AccountController.Commission), "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveAfterCommission(int orderId, [FromQuery] DashboardQuery query, CancellationToken cancellationToken)
    {
        var result = await _accounting.ApprovePayoutAsync(orderId, cancellationToken, deductCommission: true);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "อนุมัติยอดหลังหักคอมแล้ว พร้อมโอน" : result.Error;
        return ReturnToOrderStatus(query);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(SlipImage.PayoutMaxBytes + 65536)]
    public async Task<IActionResult> RecordTransfer(int orderId, DateOnly? transferDate, string? transferReference, IFormFile? slip, CancellationToken cancellationToken,
        [FromQuery] bool fromOrderStatus = false, [FromQuery] DashboardQuery? query = null)
    {
        var evidence = await SlipImage.ReadAsync(slip, cancellationToken, SlipImage.PayoutMaxBytes);
        if (evidence is null)
        {
            TempData["ErrorMessage"] = "แนบสลิป JPG หรือ PNG ไม่เกิน 10 MB";
            return fromOrderStatus ? ReturnToOrderStatus(query ?? new DashboardQuery()) :
                RedirectToAction(nameof(AccountController.Commission), "Account");
        }
        var result = await _accounting.RecordTransferAsync(orderId, transferDate, transferReference, cancellationToken, evidence);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "บันทึกการโอนเงินให้ร้านค้าแล้ว" : result.Error;
        return fromOrderStatus
            ? ReturnToOrderStatus(query ?? new DashboardQuery())
            : RedirectToAction(nameof(AccountController.Commission), "Account");
    }

    private RedirectToActionResult ReturnToOrderStatus(DashboardQuery query) =>
        RedirectToAction(nameof(AccountController.OrderStatus), "Account", new { search = query.Search, filter = query.Filter, payout = query.Payout, pageNumber = query.PageNumber });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEntry(DateOnly entryDate, string entryType, string title, decimal amount, string? note, CancellationToken cancellationToken)
    {
        var result = await _accounting.AddEntryAsync(entryDate, entryType, title, amount, note, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "บันทึกรายการแล้ว" : result.Error;
        return RedirectToAction(nameof(AccountController.Ledger), "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEntry(int id, CancellationToken cancellationToken)
    {
        var result = await _accounting.DeleteEntryAsync(id, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "ลบรายการแล้ว" : result.Error;
        return RedirectToAction(nameof(AccountController.Ledger), "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ExportLedger(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (from > to) (from, to) = (to, from);
        var report = await _accounting.GetLedgerAsync(from, to, cancellationToken);
        return File(_accounting.ToCsv(report), "text/csv; charset=utf-8", $"lovecat-ledger-{from:yyyyMMdd}-{to:yyyyMMdd}.csv");
    }
}
