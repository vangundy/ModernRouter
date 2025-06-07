using ModernRouter.Routing;
using System.Reflection;

namespace ModernRouter.Services;

/// <summary>
/// Default implementation of IRouteTableService
/// </summary>
public class RouteTableService : IRouteTableService
{
    private List<RouteEntry> _routes = new();

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

        // Build progressive paths and try to match each one
        for (int i = 0; i < segments.Length; i++)
        {
            var partialPath = "/" + string.Join("/", segments.Take(i + 1).Select(UrlEncoder.EncodeRouteParameter));
            var partialSegments = segments.Take(i + 1).ToArray();
            
            var breadcrumb = CreateBreadcrumbMatch(partialPath, partialSegments, i == segments.Length - 1);
            breadcrumbs.Add(breadcrumb);
        }

        return breadcrumbs;
    }

    private BreadcrumbRouteMatch CreateBreadcrumbMatch(string path, string[] segments, bool isActive)
    {
        var match = new BreadcrumbRouteMatch
        {
            Path = path,
            IsActive = isActive,
            Label = GetDefaultLabel(segments.Last())
        };

        // Try to find a matching route for this path
        var routeContext = RouteMatcher.Match(_routes, path);
        
        if (routeContext.Matched != null)
        {
            match.Route = routeContext.Matched;
            match.RouteValues = routeContext.RouteValues;
            
            // Check if the last segment is a parameter
            var lastSegment = segments.Last();
            var matchingTemplate = routeContext.Matched.Template;
            
            if (segments.Length <= matchingTemplate.Length)
            {
                var templateSegment = matchingTemplate[segments.Length - 1];
                if (templateSegment.IsParameter)
                {
                    match.IsParameter = true;
                    match.ParameterName = templateSegment.ParameterName;
                    
                    // Use the actual parameter value as label if available
                    if (routeContext.RouteValues.TryGetValue(templateSegment.ParameterName, out var paramValue))
                    {
                        match.Label = paramValue?.ToString() ?? lastSegment;
                    }
                }
            }
        }
        else
        {
            // No exact route match - check if this could be part of a longer route
            var couldBeParameter = CheckIfCouldBeParameter(segments);
            if (couldBeParameter.isParameter)
            {
                match.IsParameter = true;
                match.ParameterName = couldBeParameter.parameterName;
            }
        }

        return match;
    }

    private (bool isParameter, string? parameterName) CheckIfCouldBeParameter(string[] segments)
    {
        // Look for routes that could match this partial path with the last segment as a parameter
        foreach (var route in _routes)
        {
            if (route.Template.Length >= segments.Length)
            {
                bool couldMatch = true;
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    var templateSeg = route.Template[i];
                    if (templateSeg.IsParameter)
                        continue; // Parameters can match anything
                    
                    if (!templateSeg.Literal.Equals(segments[i], StringComparison.OrdinalIgnoreCase))
                    {
                        couldMatch = false;
                        break;
                    }
                }

                if (couldMatch && segments.Length <= route.Template.Length)
                {
                    var correspondingTemplate = route.Template[segments.Length - 1];
                    if (correspondingTemplate.IsParameter)
                    {
                        return (true, correspondingTemplate.ParameterName);
                    }
                }
            }
        }

        return (false, null);
    }

    private static string GetDefaultLabel(string segment)
    {
        if (string.IsNullOrEmpty(segment))
            return string.Empty;
            
        // Convert kebab-case and snake_case to Title Case
        return segment
            .Replace('-', ' ')
            .Replace('_', ' ')
            .Split(' ')
            .Select(word => string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word[1..].ToLower())
            .Aggregate((a, b) => $"{a} {b}");
    }
}