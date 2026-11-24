using WebApplication1.Domain.Identity;
using WebApplication1.Models;

namespace WebApplication1.Services.Identity;

public sealed class HttpSessionAuth : IAuthSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpSessionAuth(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session is only available during an HTTP request.");

    public void SignIn(User user, string role)
    {
        Session.SetInt32(SessionKeys.UserId, user.Id);
        RefreshProfile(user);
        SetRole(role);
    }

    public void RefreshProfile(User user)
    {
        Session.SetString(SessionKeys.UserName, user.Username);
        Session.SetString(SessionKeys.UserEmail, user.Email);
        Session.SetString(SessionKeys.FullName, user.Name);
    }

    public void SetAvatar(string avatar) => Session.SetString(SessionKeys.UserAvatar, avatar);

    public void SetRole(string role) => Session.SetString(SessionKeys.UserRole, role);

    public void SignOut() => Session.Clear();
}
