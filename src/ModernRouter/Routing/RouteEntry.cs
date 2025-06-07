using System.Reflection;

namespace ModernRouter.Routing;
public sealed record RouteEntry(RouteSegment[] Template, Type Component)
{
    public Type? LoaderType { get; init; } = Component.GetCustomAttribute<RouteDataLoaderAttribute>()?.LoaderType;
    
    /// <summary>
    /// Gets the route name if specified via RouteNameAttribute
    /// </summary>
    public string? Name { get; init; } = Component.GetCustomAttribute<RouteNameAttribute>()?.Name;
    
    /// <summary>
    /// Gets the route template as a string for URL generation
    /// </summary>
    public string TemplateString { get; init; } = string.Empty;
    
    // Store all component attributes for middleware to access without reflection
    public IReadOnlyList<Attribute> Attributes { get; init; } = [];
}
