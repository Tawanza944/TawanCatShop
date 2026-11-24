using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Models;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Orders;

namespace WebApplication1.Controllers;

[RequireLogin]
public sealed class PaymentsController(AppDbContext db, ICurrentUserService user, TimeProvider clock) : Controller
{
    [HttpPost, ValidateAntiForgeryToken, RequireRole(Roles.Accounting)]
    public async Task<IActionResult> Cancel([FromServices] BasketCancellationService cancellation, Guid id, string? reason, CancellationToken token)
    {
        var result = await cancellation.CancelAsync(id, user.RequireUserId(), reason, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Error ?? "ยกเลิกทุกออเดอร์ในยอดชำระนี้แล้ว";
        return RedirectToAction(nameof(AccountController.OrderStatus), "Account");
    }
    [HttpGet]
    public async Task<IActionResult> BasketSlip(Guid id, CancellationToken token)
    {
        var payments = db.Set<BasketPayment>().Where(p => p.Id == id);
        if (!user.IsInRole(Roles.Accounting))
        {
            if (!user.IsInRole(Roles.User)) return NotFound();
            payments = payments.Where(p => p.UserId == user.RequireUserId());
        }

        var slip = await payments.Select(p => p.Slip).SingleOrDefaultAsync(token);
        return ProtectedImage(slip);
    }

    [HttpGet]
    public async Task<IActionResult> PayoutSlip(int id, CancellationToken token)
    {
        if (!user.IsInRole(Roles.Accounting) && (!user.IsInRole(Roles.Shop) ||
            !await db.OrdersFor(user.RequireUserId()).AnyAsync(order => order.Id == id, token)))
            return NotFound();

        var slip = await db.Set<BankSlip>().SingleOrDefaultAsync(p => p.PayoutOrderId == id, token);
        return ProtectedImage(slip);
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole(Roles.Accounting)]
    public async Task<IActionResult> Review(Guid id, Guid slipId, bool approved, string? reason, CancellationToken token)
    {
        reason = reason?.Trim();
        if (!ModelState.IsValid || (!approved && (string.IsNullOrWhiteSpace(reason) || reason.Length > 300)))
        {
            TempData["ErrorMessage"] = "กรอกเหตุผลไม่เกิน 300 ตัวอักษรเมื่อขอสลิปใหม่";
            return RedirectToAction(nameof(AccountController.OrderStatus), "Account");
        }
        var actor = user.RequireUserId();
        var now = clock.GetUtcNow().UtcDateTime;
        // ตรวจรหัสสลิปด้วย เพื่อให้คำขอตรวจจากหน้าเก่าไม่ไปยืนยันสลิปคนละใบ
        var changed = await db.Set<BasketPayment>().Where(p => p.Id == id && p.Status == PaymentReview.Pending &&
                db.Set<BankSlip>().Any(s => s.PaymentId == p.Id && s.Id == slipId))
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.Status, approved ? PaymentReview.Confirmed : PaymentReview.Rejected)
                .SetProperty(p => p.ReviewedBy, actor).SetProperty(p => p.ReviewedAt, now)
                .SetProperty(p => p.Reason, approved ? null : reason), token);
        TempData[changed == 1 ? "SuccessMessage" : "ErrorMessage"] = changed == 1 ? "บันทึกผลตรวจสลิปแล้ว" : "รายการนี้ไม่ได้รอตรวจสลิป";
        return RedirectToAction(nameof(AccountController.OrderStatus), "Account");
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole(Roles.User)]
    [RequestSizeLimit(SlipImage.MaxBytes + 65536)]
    public async Task<IActionResult> Resubmit(Guid id, IFormFile? slip, CancellationToken token)
    {
        var evidence = await SlipImage.ReadAsync(slip, token);
        if (evidence is null)
        {
            TempData["ErrorMessage"] = "แนบสลิป JPG หรือ PNG ขนาดไม่เกิน 2 MB";
            return RedirectToAction(nameof(AccountController.Orders), "Account");
        }

        if (await db.Set<BankSlip>().AnyAsync(saved => saved.PaymentId != id && saved.Checksum == evidence.Checksum, token))
        {
            TempData["ErrorMessage"] = "สลิปนี้ถูกบันทึกกับรายการอื่นแล้ว";
            return RedirectToAction(nameof(AccountController.Orders), "Account");
        }

        var actor = user.RequireUserId();
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var changed = await db.Set<BasketPayment>().Where(p => p.Id == id && p.UserId == actor && p.Status == PaymentReview.Rejected)
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.Status, PaymentReview.Pending), token);
        if (changed == 1)
        {
            await db.Set<BankSlip>().Where(p => p.PaymentId == id).ExecuteDeleteAsync(token);
            evidence.PaymentId = id;
            db.Add(evidence);
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            TempData["SuccessMessage"] = "ส่งสลิปใหม่แล้ว";
        }
        else
            TempData["ErrorMessage"] = "รายการนี้ส่งสลิปใหม่ไม่ได้";

        return RedirectToAction(nameof(AccountController.Orders), "Account");
    }

    private IActionResult ProtectedImage(BankSlip? slip)
    {
        if (slip is null) return NotFound();
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(slip.Content, slip.ContentType);
    }
}
