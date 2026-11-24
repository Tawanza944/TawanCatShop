using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebApplication1.Filters;

/// <summary>
/// สร้างผลตอบกลับเมื่อคำขอถูกปฏิเสธ โดยเลือก JSON หรือหน้าเว็บตามประเภทคำขอ
/// </summary>
internal static class AuthorizationResults
{
    public static IActionResult NotSignedIn(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        if (IsApiRequest(request))
        {
            return new JsonResult(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }

        // เก็บเฉพาะหน้า GET เป็นหน้าที่กลับไปได้หลังล็อกอิน เพราะ POST ต้องใช้ข้อมูลฟอร์มเดิม
        var returnUrl = HttpMethods.IsGet(request.Method) ? $"{request.PathBase}{request.Path}{request.QueryString}" : null;
        return new RedirectToActionResult("Login", "Home", new { returnUrl });
    }

    public static IActionResult Forbidden(AuthorizationFilterContext context)
    {
        if (IsApiRequest(context.HttpContext.Request))
        {
            return new JsonResult(new { success = false, message = "บัญชีนี้ไม่มีสิทธิ์ทำรายการนี้" }) { StatusCode = StatusCodes.Status403Forbidden };
        }

        return new ViewResult
        {
            ViewName = "Forbidden",
            StatusCode = StatusCodes.Status403Forbidden,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
            {
                ["Title"] = "ไม่มีสิทธิ์เข้าถึง"
            }
        };
    }

    private static bool IsApiRequest(HttpRequest request) =>
        request.HasJsonContentType()
        || request.Headers.Accept.Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        || string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
