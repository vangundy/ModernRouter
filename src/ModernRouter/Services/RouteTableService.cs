using ModernRouter.Routing;
using System.Reflection;

namespace ModernRouter.Services;

/// <summary>
/// Default implementation of IRouteTableService
/// </summary>
public class RouteTableService : IRouteTableService
{
    private List<RouteEntry> _routes = [];

    /// <inheritdoc />
    public IReadOnlyList<RouteEntry> Routes => _routes.AsReadOnly();

    /// <inheritdoc />
    public void Initialize(IEnumerable<Assembly> assemblies)
    {
        _routes = RouteTableFactory.Build(assemblies);
    }

    /// <inheritdoc />
    public RouteContext MatchRoute(string path)
    {
        return RouteMatcher.Match(_routes, path);
    }

    /// <inheritdoc />
    public IList<BreadcrumbRouteMatch> GetBreadcrumbMatches(string path)
    {
        var breadcrumbs = new List<BreadcrumbRouteMatch>();
        
        // Separate path from query string
        var questionMarkIndex = path.IndexOf('?');
        var pathPart = questionMarkIndex >= 0 ? path[..questionMarkIndex] : path;
        
        var segments = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries)
                              .Select(UrlEncoder.DecodeRouteParameter)
                              .ToArray();

        if (segments.Length == 0)
            return breadcrumbs;

        // Find all routes that could contribute to the breadcrumb hierarchy
        var matchedRoutes = new List<(string path, RouteEntry route, RouteContext context, bool isActive)>();
        
        // Try matching progressively longer paths, but only keep routes that actually match
        for (int i = 1; i <= segments.Length; i++)
        {
            var partialPath = "/" + string.Join("/", segments.Take(i).Select(UrlEncoder.EncodeRouteParameter));
            var routeContext = RouteMatcher.Match(_routes, partialPath);
            
            if (routeContext.Matched != null)
            {
                var isActive = i == segments.Length;
                matchedRoutes.Add((partialPath, routeContext.Matched, routeContext, isActive));
            }
        }

        // Create breadcrumbs from the matched routes
        foreach (var (matchPath, route, context, isActive) in matchedRoutes)
        {
            var breadcrumb = CreateBreadcrumbMatchFromRoute(route, context, matchPath, isActive);
            breadcrumbs.Add(breadcrumb);
        }

        return breadcrumbs;
    }

    private BreadcrumbRouteMatch CreateBreadcrumbMatchFromRoute(RouteEntry route, RouteContext context, string path, bool isActive)
    {
        var match = new BreadcrumbRouteMatch
        {
            Path = path,
            IsActive = isActive,
            Route = route,
            RouteValues = context.RouteValues
        };

        // Get the breadcrumb metadata from the route
        var breadcrumbMetadata = route.Component.GetCustomAttribute<BreadcrumbAttribute>();
        if (breadcrumbMetadata != null)
        {
            match.Label = breadcrumbMetadata.Title;
        }
        else
        {
            // Fall back to a reasonable default based on the route pattern
            var literalSegments = route.Template.Where(t => !t.IsParameter).Select(t => t.Literal).ToArray();
            if (literalSegments.Length > 0)
            {
                var lastLiteral = literalSegments.Last();
                match.Label = ConvertToTitleCase(lastLiteral);
            }
            else
            {
                match.Label = "Page";
            }
        }

        return match;
    }

    private static string ConvertToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace('-', ' ')
            .Replace('_', ' ')
            .Split(' ')
            .Select(word => string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word[1..].ToLower())
            .Aggregate((a, b) => $"{a} {b}");
    }
}