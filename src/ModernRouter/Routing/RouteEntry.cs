using System.Reflection;

namespace ModernRouter.Routing;
public sealed record RouteEntry(RouteSegment[] Template, Type Component)
{
    public Type? LoaderType { get; init; } = Component.GetCustomAttribute<RouteDataLoaderAttribute>()?.LoaderType;
}
