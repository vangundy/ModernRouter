using ModernRouter.Components;
using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Advanced breadcrumb service that understands route hierarchy and outlet nesting
/// </summary>
public interface IHierarchicalBreadcrumbService
{
    /// <summary>
    /// Generates breadcrumbs based on route hierarchy and component nesting
    /// </summary>
    /// <param name="currentPath">The current URL path</param>
    /// <param name="options">Breadcrumb options</param>
    /// <returns>Hierarchical breadcrumb items</returns>
    IList<BreadcrumbItem> GenerateHierarchicalBreadcrumbs(string currentPath, BreadcrumbOptions? options = null);

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