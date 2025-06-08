using ModernRouter.Components;
using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Service for managing breadcrumb generation and customization with support for both basic and hierarchical breadcrumbs
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Generates breadcrumbs for the specified path using basic route table matching
    /// </summary>
    /// <param name="path">The path to generate breadcrumbs for</param>
    /// <param name="options">Breadcrumb options</param>
    /// <returns>List of breadcrumb items</returns>
    IList<BreadcrumbItem> GenerateBreadcrumbs(string path, BreadcrumbOptions? options = null);

    /// <summary>
    /// Generates breadcrumbs based on route hierarchy and component nesting
    /// </summary>
    /// <param name="currentPath">The current URL path</param>
    /// <param name="options">Breadcrumb options</param>
    /// <returns>Hierarchical breadcrumb items</returns>
    IList<BreadcrumbItem> GenerateHierarchicalBreadcrumbs(string currentPath, BreadcrumbOptions? options = null);

    /// <summary>
    /// Registers a custom breadcrumb provider
    /// </summary>
    /// <param name="provider">The breadcrumb provider to register</param>
    void RegisterProvider(IBreadcrumbProvider provider);

    /// <summary>
    /// Removes a breadcrumb provider
    /// </summary>
    /// <param name="provider">The breadcrumb provider to remove</param>
    void RemoveProvider(IBreadcrumbProvider provider);

    /// <summary>
    /// Gets breadcrumb metadata for a route entry
    /// </summary>
    /// <param name="routeEntry">The route entry</param>
    /// <returns>Breadcrumb metadata if available</returns>
    BreadcrumbAttribute? GetBreadcrumbMetadata(RouteEntry routeEntry);

    /// <summary>
    /// Discovers route hierarchies by analyzing route patterns and component relationships
    /// </summary>
    /// <param name="routes">Available routes</param>
    /// <returns>Route hierarchy tree</returns>
    RouteHierarchy BuildRouteHierarchy(IEnumerable<RouteEntry> routes);

    /// <summary>
    /// Finds the best matching parent routes for a given route
    /// </summary>
    /// <param name="route">The route to find parents for</param>
    /// <param name="hierarchy">The route hierarchy</param>
    /// <returns>Parent routes in order from root to immediate parent</returns>
    IList<RouteEntry> FindParentRoutes(RouteEntry route, RouteHierarchy hierarchy);

    /// <summary>
    /// Resolves breadcrumb metadata with parameter substitution
    /// </summary>
    /// <param name="route">The route entry</param>
    /// <param name="routeValues">Current route parameter values</param>
    /// <returns>Resolved breadcrumb item</returns>
    BreadcrumbItem ResolveBreadcrumbItem(RouteEntry route, Dictionary<string, object?> routeValues);

    /// <summary>
    /// Gets the current route hierarchy (may be null if not yet built)
    /// </summary>
    RouteHierarchy? CurrentHierarchy { get; }

    /// <summary>
    /// Event fired when breadcrumbs are generated
    /// </summary>
    event EventHandler<BreadcrumbGeneratedEventArgs>? BreadcrumbsGenerated;
}

/// <summary>
/// Interface for custom breadcrumb providers
/// </summary>
public interface IBreadcrumbProvider
{
    /// <summary>
    /// Gets the priority of this provider (higher = executed first)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Determines if this provider can handle the specified route
    /// </summary>
    /// <param name="routeMatch">The route match</param>
    /// <returns>True if this provider can handle the route</returns>
    bool CanHandle(BreadcrumbRouteMatch routeMatch);

    /// <summary>
    /// Generates a breadcrumb item for the specified route match
    /// </summary>
    /// <param name="routeMatch">The route match</param>
    /// <param name="options">Breadcrumb options</param>
    /// <returns>Generated breadcrumb item</returns>
    BreadcrumbItem? GenerateBreadcrumb(BreadcrumbRouteMatch routeMatch, BreadcrumbOptions options);
}

/// <summary>
/// Event arguments for breadcrumb generation events
/// </summary>
public class BreadcrumbGeneratedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the path that breadcrumbs were generated for
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the generated breadcrumb items
    /// </summary>
    public IList<BreadcrumbItem> Breadcrumbs { get; }

    /// <summary>
    /// Gets the options used for generation
    /// </summary>
    public BreadcrumbOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the BreadcrumbGeneratedEventArgs
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="breadcrumbs">The breadcrumb items</param>
    /// <param name="options">The options</param>
    public BreadcrumbGeneratedEventArgs(string path, IList<BreadcrumbItem> breadcrumbs, BreadcrumbOptions options)
    {
        Path = path;
        Breadcrumbs = breadcrumbs;
        Options = options;
    }
}

/// <summary>
/// Represents the hierarchical structure of routes
/// </summary>
public class RouteHierarchy
{
    public Dictionary<RouteEntry, RouteNode> Nodes { get; } = new();
    public List<RouteNode> RootNodes { get; } = new();

    public void AddRoute(RouteEntry route, RouteEntry? parent = null)
    {
        if (!Nodes.TryGetValue(route, out var node))
        {
            node = new RouteNode(route);
            Nodes[route] = node;
        }

        if (parent != null && Nodes.TryGetValue(parent, out var parentNode))
        {
            node.Parent = parentNode;
            parentNode.Children.Add(node);
        }
        else
        {
            RootNodes.Add(node);
        }
    }

    public RouteNode? FindNode(RouteEntry route) => Nodes.GetValueOrDefault(route);
}

/// <summary>
/// Represents a node in the route hierarchy tree
/// </summary>
public class RouteNode
{
    public RouteEntry Route { get; }
    public RouteNode? Parent { get; set; }
    public List<RouteNode> Children { get; } = new();
    public int Depth => Parent?.Depth + 1 ?? 0;

    public RouteNode(RouteEntry route)
    {
        Route = route;
    }

    public IEnumerable<RouteNode> GetAncestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public IEnumerable<RouteNode> GetPath()
    {
        var path = GetAncestors().Reverse().ToList();
        path.Add(this);
        return path;
    }
}