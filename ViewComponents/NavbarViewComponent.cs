using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domain.Identity;
using WebApplication1.Services.Identity;

namespace WebApplication1.ViewComponents;

public sealed record NavbarViewModel(
    LayoutVariant Variant,
    bool IsAuthenticated,
    string? DisplayName,
    string? AvatarUrl,
    RoleProfile? Role,
    bool ShowShoppingTools);

/// <summary>เตรียมเมนูด้านบนและอ่านสถานะล็อกอินผ่าน <see cref="ICurrentUserService"/></summary>
public sealed class NavbarViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUser;

    public NavbarViewComponent(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public IViewComponentResult Invoke(bool compact = false)
    {
        var role = _currentUser.IsAuthenticated ? RoleProfiles.For(_currentUser.Role) : null;
        var avatar = HttpContext.Session.GetString(SessionKeys.UserAvatar);
        var model = new NavbarViewModel(
            compact ? LayoutVariant.Compact : LayoutVariant.Full,
            _currentUser.IsAuthenticated,
            _currentUser.DisplayName,
            avatar is not null && avatar.StartsWith('/') ? avatar : null,
            role,
            ShowShoppingTools: role is null || role.Shops);

        return View(compact ? "Compact" : "Default", model);
    }
}
