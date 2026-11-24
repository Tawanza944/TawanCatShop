namespace WebApplication1.Services.Identity;

/// <summary>
/// Read-only view of the signed-in user for the current request.
/// Controllers, filters and view components depend on this instead of reading the session directly.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    string? UserName { get; }

    string? DisplayName { get; }

    /// <summary>Profile picture path, or null when the user has not uploaded one.</summary>
    string? AvatarUrl { get; }

    /// <summary>The user's role; <see cref="Domain.Identity.Roles.User"/> when signed in without a staff role.</summary>
    string? Role { get; }

    bool IsInRole(params string[] roles);

    /// <summary>The signed-in user's id. Throws when called for an anonymous request.</summary>
    int RequireUserId();
}
