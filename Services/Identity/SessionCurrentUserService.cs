using WebApplication1.Domain.Identity;

namespace WebApplication1.Services.Identity;

public sealed class SessionCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public int? UserId => Session?.GetInt32(SessionKeys.UserId);

    public bool IsAuthenticated => UserId.HasValue;

    public string? UserName => Session?.GetString(SessionKeys.UserName);

    public string? DisplayName => Session?.GetString(SessionKeys.FullName) ?? UserName;

    public string? AvatarUrl => Session?.GetString(SessionKeys.UserAvatar);

    public string? Role => IsAuthenticated ? Session?.GetString(SessionKeys.UserRole) ?? Roles.User : null;

    public bool IsInRole(params string[] roles)
    {
        var current = Role;
        return current is not null && roles.Any(role => Roles.AreEqual(role, current));
    }

    public int RequireUserId() =>
        UserId ?? throw new InvalidOperationException("This operation requires a signed-in user.");
}
