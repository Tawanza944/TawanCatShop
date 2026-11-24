using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Shipping;
using WebApplication1.Filters;
using WebApplication1.Services.Orders;

namespace WebApplication1.Controllers;

/// <summary>
/// รับฟอร์ม “ยืนยันส่งพัสดุ” จากหน้าแพ็กสินค้า แล้วกลับไปหน้าหลังร้านด้วย Post/Redirect/Get
/// Controller แปลงค่าจากฟอร์มเป็น <see cref="ShipmentDetails"/> และแจ้งผลแก่ร้าน
/// ส่วนกฎการบันทึกอยู่ในโมเดลโดเมนและ <see cref="IShipmentService"/>
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
