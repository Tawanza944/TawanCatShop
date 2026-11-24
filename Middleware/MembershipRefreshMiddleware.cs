using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Services.Identity;

namespace WebApplication1.Middleware;

/// <summary>
/// Re-reads the signed-in user's role and status on every request so that an admin's change
/// (new role, suspension, deleted account) takes effect immediately instead of at the next sign-in.
/// </summary>
public sealed class MembershipRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public MembershipRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser, IAuthSession authSession, AppDbContext db)
    {
        if (currentUser.UserId is int userId && !IsStaticFile(context.Request.Path))
        {
            var membership = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    Staff = db.Staff.Where(s => s.UserId == u.Id)
                        .Select(s => new { s.Status, RoleName = db.Roles.Where(r => r.Id == s.RoleId).Select(r => r.Name).FirstOrDefault() })
                        .FirstOrDefault()
                })
                .SingleOrDefaultAsync(context.RequestAborted);

            if (membership is null || membership.Staff?.Status == StaffStatus.Suspended)
            {
                authSession.SignOut();
            }
            else
            {
                var role = membership.Staff?.RoleName ?? Roles.User;
                if (!Roles.AreEqual(role, currentUser.Role))
                {
                    authSession.SetRole(role);
                }
            }
        }

        await _next(context);
    }

    private static bool IsStaticFile(PathString path) => Path.HasExtension(path.Value);
}
