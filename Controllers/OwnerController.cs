using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Backoffice;

namespace WebApplication1.Controllers;

[RequireRole(Roles.Owner)]
public sealed class OwnerController(IUserAdminService users) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteAdmin(int id, CancellationToken cancellationToken)
    {
        var result = await users.DemoteAdminAsync(id, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "เปลี่ยนบทบาท Admin เป็นลูกค้าแล้ว" : result.Error;
        return RedirectToAction(nameof(AccountController.AdminAccess), "Account");
    }
}
