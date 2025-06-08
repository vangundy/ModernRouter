using System.Reflection;

namespace ModernRouter.Routing;

/// <summary>
/// Represents a route entry with its primary template and optional aliases
/// </summary>
public sealed record RouteEntry(RouteSegment[] Template, Type Component)
{
    public Type? LoaderType { get; init; } = Component.GetCustomAttribute<RouteDataLoaderAttribute>()?.LoaderType;
    
    /// <summary>
    /// Gets the route name if specified via RouteNameAttribute
    /// </summary>
    public string? Name { get; init; } = Component.GetCustomAttribute<RouteNameAttribute>()?.Name;
    
    /// <summary>
    /// Gets the primary route template as a string for URL generation
    /// </summary>
    public string TemplateString { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the list of route aliases for this entry
    /// </summary>
    public IReadOnlyList<RouteAlias> Aliases { get; init; } = [];
    
    // Store all component attributes for middleware to access without reflection
    public IReadOnlyList<Attribute> Attributes { get; init; } = [];
}

/// <summary>
/// Represents a route alias with its template and configuration
/// </summary>
public sealed record RouteAlias
{
    /// <summary>
    /// Gets the parsed template segments for this alias
    /// </summary>
    public RouteSegment[] Template { get; init; } = [];
    
    /// <summary>
    /// Gets the template string for this alias
    /// </summary>
    public string TemplateString { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets whether this alias should redirect to the primary route
    /// </summary>
    public bool RedirectToPrimary { get; init; } = false;
    
    /// <summary>
    /// Gets the priority of this alias for matching
    /// </summary>
    public int Priority { get; init; } = 0;
}
