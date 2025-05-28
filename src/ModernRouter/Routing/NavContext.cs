namespace ModernRouter.Routing;
public sealed class NavContext
{
    public string TargetUri { get; init; } = "";
    public RouteContext? Match { get; init; }
    public CancellationToken CancellationToken { get; init; }
}