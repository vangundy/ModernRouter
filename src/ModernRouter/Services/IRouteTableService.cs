using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Service interface for accessing the route table
/// </summary>
public interface IRouteTableService
{
    /// <summary>
    /// Gets the current route table
    /// </summary>
    IReadOnlyList<RouteEntry> Routes { get; }

    /// <summary>
    /// Initializes the route table with the specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for routes</param>
    void Initialize(IEnumerable<System.Reflection.Assembly> assemblies);

    /// <summary>
    /// Finds the best matching route for the given path
    /// </summary>
    /// <param name="path">Path to match</param>
    /// <returns>Route context with match information</returns>
    RouteContext MatchRoute(string path);

    /// <summary>
    /// Gets all possible partial matches for breadcrumb generation
    /// </summary>
    /// <param name="path">Full path to match</param>
    /// <returns>List of partial route matches for breadcrumbs</returns>
    IList<BreadcrumbRouteMatch> GetBreadcrumbMatches(string path);
}

/// <summary>
/// Represents a route match for breadcrumb generation
/// </summary>
public class BreadcrumbRouteMatch
{
    /// <summary>
    /// The partial path for this breadcrumb segment
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The matched route entry, if any
    /// </summary>
    public RouteEntry? Route { get; set; }

    /// <summary>
    /// The route values for this match
    /// </summary>
    public Dictionary<string, object?> RouteValues { get; set; } = new();

    /// <summary>
    /// Whether this segment represents a route parameter
    /// </summary>
    public bool IsParameter { get; set; }

    /// <summary>
    /// The parameter name if this is a parameter segment
    /// </summary>
    public string? ParameterName { get; set; }

    /// <summary>
    /// The display label for this breadcrumb
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the final/active breadcrumb
    /// </summary>
    public bool IsActive { get; set; }
}