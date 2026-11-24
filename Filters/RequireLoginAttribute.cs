using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication1.Services.Identity;

namespace WebApplication1.Filters;

/// <summary>
/// Allows the action only for a signed-in user. Honors <see cref="AllowAnonymousAttribute"/> on the action.
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
