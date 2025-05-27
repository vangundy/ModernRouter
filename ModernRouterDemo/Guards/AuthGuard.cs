using ModernRouter.Routing;

namespace ModernRouterDemo.Guards;

//public sealed class AuthGuard : INavMiddleware
//{
//    private readonly IAuthService _auth;
//    public AuthGuard(IAuthService auth) => _auth = auth;

//    public Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next)
//    {
//        // protect any path that starts with "admin"
//        if (ctx.Match?.Matched is not null &&
//            ctx.Match.Matched.Component.Namespace?.Contains(".Admin") == true &&
//            !_auth.IsSignedIn)
//        {
//            return Task.FromResult(NavResult.Redirect("/login"));
//        }
//        return next();
//    }
//}
