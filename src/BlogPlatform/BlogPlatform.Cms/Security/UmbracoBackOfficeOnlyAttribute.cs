using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;

namespace BlogPlatform.Cms.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class UmbracoBackOfficeOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowsAnonymous = context.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (allowsAnonymous)
        {
            return;
        }

        var authenticationService = context.HttpContext.RequestServices
            .GetRequiredService<IAuthenticationService>();

        var result = await authenticationService.AuthenticateAsync(
            context.HttpContext,
            Constants.Security.BackOfficeAuthenticationType);

        if (result.Succeeded &&
            result.Principal?.Identity?.IsAuthenticated == true)
        {
            context.HttpContext.User = result.Principal;
            return;
        }

        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Please sign in to Umbraco before using Blog Admin."
            });

            return;
        }

        var returnUrl = context.HttpContext.Request.PathBase
            + context.HttpContext.Request.Path
            + context.HttpContext.Request.QueryString;

        context.Result = new RedirectResult(
            "/umbraco/login?ReturnUrl=" + Uri.EscapeDataString(returnUrl));
    }
}
