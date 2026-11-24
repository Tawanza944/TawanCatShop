using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Backoffice;
using WebApplication1.Services.Identity;

namespace WebApplication1.Controllers;

[RequireRole(Roles.User, Roles.Shop)]
public sealed class ShopContactController(ShopInteractions interactions, ICurrentUserService user) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Conversation(int id, CancellationToken token)
    {
        var conversation = await interactions.ConversationAsync(user.RequireUserId(), user.IsInRole(Roles.Shop), id, token);
        return conversation is null ? NotFound() : View(conversation);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id, string? message, CancellationToken token)
    {
        var result = await interactions.SendAsync(user.RequireUserId(), user.IsInRole(Roles.Shop), id, message, token);
        if (!result.Succeeded) TempData["ErrorMessage"] = result.Error;
        return RedirectToAction(nameof(Conversation), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole(Roles.User)]
    public async Task<IActionResult> Follow(int id, bool follow, CancellationToken token)
    {
        await interactions.SetFollowAsync(user.RequireUserId(), id, follow, token);
        return RedirectToAction(nameof(HomeController.Shop), "Home", new { id });
    }
}
