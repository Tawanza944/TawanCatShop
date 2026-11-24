using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Shipping;
using WebApplication1.Filters;
using WebApplication1.Services.Orders;

namespace WebApplication1.Controllers;

/// <summary>
/// The shop's "ยืนยันส่งพัสดุ" form on the packing page posts here (Post/Redirect/Get).
/// The controller only turns form fields into <see cref="ShipmentDetails"/> and reports the result;
/// the rules live in the domain value and in <see cref="IShipmentService"/>.
/// </summary>
[RequireRole(Roles.Shop)]
public sealed class ShipmentsController : Controller
{
    private readonly IShipmentService _shipments;

    public ShipmentsController(IShipmentService shipments)
    {
        _shipments = shipments;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dispatch(string orderNo, string? carrier, string? otherCarrierName, string? trackingNumber,
        CancellationToken cancellationToken)
    {
        var details = ShipmentDetails.Create(carrier, otherCarrierName, trackingNumber);
        if (!details.Succeeded)
            return BackToPacking(error: $"ออเดอร์ {orderNo}: {details.Error}");

        var result = await _shipments.DispatchAsync(orderNo, details.Value!, cancellationToken);
        return result.Succeeded
            ? BackToPacking(success: $"ส่งพัสดุออเดอร์ {orderNo} แล้ว ทาง {details.Value!.CarrierDisplayName} เลขพัสดุ {details.Value.TrackingNumber}")
            : BackToPacking(error: $"ออเดอร์ {orderNo}: {result.Error}");
    }

    private RedirectToActionResult BackToPacking(string? success = null, string? error = null)
    {
        TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? success;
        return RedirectToAction(nameof(AccountController.Packing), "Account");
    }
}
