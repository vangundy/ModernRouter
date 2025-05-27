using ModernRouter.Routing;

namespace ModernRouterDemo.Guards;

//public sealed class AnalyticsTap : INavMiddleware
//{
//    private readonly IAnalytics _ga;
//    public AnalyticsTap(IAnalytics ga) => _ga = ga;

//    public async Task<NavResult> InvokeAsync(NavContext ctx,
//                                             Func<Task<NavResult>> next)
//    {
//        var result = await next();
//        if (!result.IsCancelled)
//            _ga.TrackPageView(ctx.TargetUri);
//        return result;
//    }
//}
