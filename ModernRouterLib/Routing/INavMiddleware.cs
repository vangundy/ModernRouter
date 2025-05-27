namespace ModernRouter.Routing;
public interface INavMiddleware
{
    Task<NavResult> InvokeAsync(NavContext context, Func<Task<NavResult>> next);
}
