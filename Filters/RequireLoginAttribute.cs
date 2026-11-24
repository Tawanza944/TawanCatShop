using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication1.Services.Identity;

namespace WebApplication1.Filters;

/// <summary>
/// ให้เฉพาะผู้ที่ล็อกอินเรียก Action ยกเว้น Action ที่ระบุ <see cref="AllowAnonymousAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireLoginAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        if (!currentUser.IsAuthenticated)
        {
            context.Result = AuthorizationResults.NotSignedIn(context);
            return;
        }

        if (!IsAllowed(currentUser))
        {
            context.Result = AuthorizationResults.Forbidden(context);
        }
    }

    protected virtual bool IsAllowed(ICurrentUserService currentUser) => true;
}
