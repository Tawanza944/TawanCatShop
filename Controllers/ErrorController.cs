using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ViewModels.Shared;

namespace WebApplication1.Controllers;

/// <summary>
/// Friendly pages for 404, 500 and other status codes. Reached through
/// UseStatusCodePagesWithReExecute and UseExceptionHandler in Program.cs.
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("Error/Status/{statusCode:int}")]
    public IActionResult Status(int statusCode)
    {
        if (statusCode >= 500 && HttpContext.Features.Get<IExceptionHandlerPathFeature>() is { } failure)
        {
            _logger.LogError(failure.Error, "Unhandled exception on {Path}.", failure.Path);
        }

        Response.StatusCode = statusCode;
        var model = statusCode switch
        {
            404 => new StatusPageModel(404, "หาหน้านี้ไม่เจอ", "ลิงก์อาจพิมพ์ผิด หรือหน้านี้ถูกย้ายไปแล้ว ลองกลับไปหน้าแรกแล้วค้นหาอีกครั้ง", CatPose.Sleeping),
            403 => new StatusPageModel(403, "หน้านี้ไม่ได้เปิดให้บัญชีของคุณนะ", "บัญชีที่ใช้อยู่ไม่มีสิทธิ์ดูหน้านี้", CatPose.Peeking),
            >= 500 => new StatusPageModel(statusCode, "ระบบสะดุดนิดหน่อย", "เราบันทึกปัญหาไว้แล้ว ลองใหม่อีกครั้งในอีกสักครู่", CatPose.Stretching),
            _ => new StatusPageModel(statusCode, "เกิดข้อผิดพลาด", "ลองกลับไปหน้าแรกแล้วทำรายการใหม่อีกครั้ง", CatPose.Sitting)
        };

        ViewData["Title"] = model.Title;
        return View(model);
    }
}
