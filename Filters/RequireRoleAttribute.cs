using WebApplication1.Services.Identity;

namespace WebApplication1.Filters;

/// <summary>
/// อนุญาตให้ผู้ใช้ที่ล็อกอินและมีบทบาทใน <see cref="AllowedRoles"/> เรียก Action ได้
/// ตัวอย่าง <c>[RequireRole(Roles.Shop, Roles.Raider)]</c>
/// </summary>
public sealed class RequireRoleAttribute : RequireLoginAttribute
{
    public RequireRoleAttribute(params string[] allowedRoles)
    {
        if (allowedRoles.Length == 0)
        {
            throw new ArgumentException("At least one role is required.", nameof(allowedRoles));
        }

        AllowedRoles = allowedRoles;
    }

    public IReadOnlyList<string> AllowedRoles { get; }

    protected override bool IsAllowed(ICurrentUserService currentUser) => currentUser.IsInRole([.. AllowedRoles]);
}
