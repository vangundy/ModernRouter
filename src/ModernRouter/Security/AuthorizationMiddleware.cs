using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Security;

public class AuthorizationMiddleware : INavMiddleware
{
    private readonly IAuthorizationService _authService;
    private readonly NavigationManager _navManager;

    public AuthorizationMiddleware(IAuthorizationService authService, NavigationManager navManager)
    {
        _authService = authService;
        _navManager = navManager;
    }

    public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next)
    {
        // Check for cancellation before processing
        ctx.CancellationToken.ThrowIfCancellationRequested();
        
        if (ctx.Match?.Matched is null)
            return await next();

        // Check if the route has AllowAnonymous attribute
        var allowAnonymous = ctx.Match.Matched.Attributes
                               .Any(a => a is AllowAnonymousAttribute);
        if (allowAnonymous)
            return await next();

        // Check if the route has any Authorize attributes
        var authorizeAttrs = ctx.Match.Matched.Attributes
                                .OfType<AuthorizeAttribute>()
                                .ToArray();
        if (authorizeAttrs.Length == 0)
            return await next(); // No authorization required

        // Check for cancellation before authentication check
        ctx.CancellationToken.ThrowIfCancellationRequested();

        // Route requires authorization
        if (!_authService.IsAuthenticated())
        {
            var returnUrl = Uri.EscapeDataString(_navManager.Uri);
            return NavResult.Redirect($"/login?returnUrl={returnUrl}");
        }

        // Check roles and policies
        foreach (var attr in authorizeAttrs)
        {
            // Check for cancellation during policy evaluation
            ctx.CancellationToken.ThrowIfCancellationRequested();
            
            if (!string.IsNullOrEmpty(attr.Roles))
            {
                var roles = attr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (!_authService.IsInRoles(roles))
                    return NavResult.Redirect("/unauthorized");
            }

            if (!string.IsNullOrEmpty(attr.Policy))
            {
                var authorized = await _authService.AuthorizeAsync(attr.Policy);
                if (!authorized)
                    return NavResult.Redirect("/unauthorized");
            }
        }

        return await next();
    }
}