using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Filters;
using WebApplication1.Services.Orders;
using WebApplication1.Services.Identity;

namespace WebApplication1.Controllers;

/// <summary>
/// Order actions submitted from dashboard forms (Tag Helper forms, Post/Redirect/Get).
/// Who may do what is decided by <see cref="IOrderWorkflow"/> inside the service.
/// </summary>
public sealed class OrdersController : Controller
{
    private readonly IOrderFulfillmentService _fulfillment;
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public OrdersController(IOrderFulfillmentService fulfillment, AppDbContext db, ICurrentUserService currentUser)
    {
        _fulfillment = fulfillment;
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Printable shipping label for one order. Opens in a new tab and is printed with the browser.</summary>
    [HttpGet]
    [RequireRole(Roles.Shop)]
    public async Task<IActionResult> Label(string orderNo, CancellationToken cancellationToken)
    {
        var order = await _db.OrdersFor(_currentUser.RequireUserId()).AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNo == orderNo, cancellationToken);
        return order is null || order.Status == OrderStatus.Cancelled ? NotFound() : View(order);
    }

    [HttpPost]
    [RequireRole(Roles.Shop, Roles.Raider, Roles.User)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveTo(string orderNo, string status, string? returnSection, CancellationToken cancellationToken)
    {
        if (!OrderStatusCodes.TryParseCode(status, out var nextStatus))
        {
            TempData["ErrorMessage"] = "สถานะไม่ถูกต้อง";
        }
        else
        {
            var result = await _fulfillment.MoveToAsync(orderNo, nextStatus, cancellationToken);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? $"ออเดอร์ {orderNo}: {nextStatus.ToThai()}"
                : result.Error;
        }

        return RedirectToAction(WebApplication1.ViewModels.Dashboard.DashboardRoutes.ActionFor(returnSection ?? "overview"), "Account");
    }

    [HttpPost]
    [RequireRole(Roles.User, Roles.Shop)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string orderNo, string? returnSection, CancellationToken cancellationToken)
    {
        var result = await _fulfillment.CancelAsync(orderNo, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? $"ยกเลิกออเดอร์ {orderNo} แล้ว คืนสต็อกและสิทธิ์คูปองเรียบร้อย"
            : result.Error;

        return RedirectToAction(WebApplication1.ViewModels.Dashboard.DashboardRoutes.ActionFor(returnSection ?? "orders"), "Account");
    }
}
