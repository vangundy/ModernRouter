namespace ModernRouter.Routing;

/// <summary>
/// Represents the result of route matching, including information about aliases
/// </summary>
public sealed class RouteContext
{
    public RouteEntry? Matched { get; init; }
    public string[] RemainingSegments { get; init; } = [];
    public Dictionary<string, object?> RouteValues { get; init; } = [];
    public QueryParameters QueryParameters { get; init; } = new();
    
    /// <summary>
    /// Gets whether this route was matched via an alias rather than the primary route
    /// </summary>
    public bool IsAliasMatch { get; init; } = false;
    
    /// <summary>
    /// Gets the matched alias information if this was an alias match
    /// </summary>
    public RouteAlias? MatchedAlias { get; init; }
    
    /// <summary>
    /// Gets the template string that was actually matched (either primary or alias)
    /// </summary>
    public string MatchedTemplate => IsAliasMatch ? MatchedAlias?.TemplateString ?? "" : Matched?.TemplateString ?? "";
}
