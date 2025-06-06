namespace ModernRouter.Routing;
public sealed class RouteContext
{
    public RouteEntry? Matched { get; init; }
    public string[] RemainingSegments { get; init; } = [];
    public Dictionary<string, object?> RouteValues { get; init; } = [];
    public QueryParameters QueryParameters { get; init; } = new();
}
