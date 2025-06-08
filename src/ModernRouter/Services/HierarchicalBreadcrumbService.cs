using ModernRouter.Components;
using ModernRouter.Routing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ModernRouter.Services;

/// <summary>
/// Advanced breadcrumb service that understands route hierarchy and outlet nesting
/// </summary>
public class HierarchicalBreadcrumbService : IHierarchicalBreadcrumbService
{
    private readonly IRouteTableService _routeTableService;
    private RouteHierarchy? _cachedHierarchy;

    public HierarchicalBreadcrumbService(IRouteTableService routeTableService)
    {
        _routeTableService = routeTableService ?? throw new ArgumentNullException(nameof(routeTableService));
    }

    /// <inheritdoc />
    public IList<BreadcrumbItem> GenerateHierarchicalBreadcrumbs(string currentPath, BreadcrumbOptions? options = null)
    {
        options ??= new BreadcrumbOptions();
        var breadcrumbs = new List<BreadcrumbItem>();

        // Add home breadcrumb if configured
        if (options.IncludeHome)
        {
            breadcrumbs.Add(new BreadcrumbItem
            {
                Label = options.HomeLabel,
                Url = options.HomeUrl,
                IsActive = false,
                Icon = "home",
                Order = 0
            });
        }

        // Clean path and get route context
        var cleanPath = CleanPath(currentPath);
        var routeContext = _routeTableService.MatchRoute(cleanPath);
        
        if (routeContext.Matched == null)
            return breadcrumbs;

        // Build hierarchy if not cached
        _cachedHierarchy ??= BuildRouteHierarchy(_routeTableService.Routes);

        // Find the route hierarchy path
        var routeNode = _cachedHierarchy.FindNode(routeContext.Matched);
        if (routeNode == null)
            return breadcrumbs;

        // Get all routes in the hierarchy path
        var hierarchyPath = routeNode.GetPath().ToList();

        // Generate breadcrumbs for each level in the hierarchy
        var currentSegments = cleanPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var consumedSegments = 0;

        foreach (var node in hierarchyPath)
        {
            var route = node.Route;
            var breadcrumbPath = BuildPathForRoute(route, currentSegments, ref consumedSegments, routeContext.RouteValues);
            
            if (!string.IsNullOrEmpty(breadcrumbPath))
            {
                var isActive = node == hierarchyPath.Last();
                var breadcrumbItem = ResolveBreadcrumbItem(route, routeContext.RouteValues);
                breadcrumbItem.Url = "/" + breadcrumbPath;
                breadcrumbItem.IsActive = isActive;
                breadcrumbItem.Order = breadcrumbs.Count + (options.IncludeHome ? 1 : 0);
                
                breadcrumbs.Add(breadcrumbItem);
            }
        }

        return breadcrumbs;
    }

    /// <inheritdoc />
    public RouteHierarchy BuildRouteHierarchy(IEnumerable<RouteEntry> routes)
    {
        var hierarchy = new RouteHierarchy();
        var routeList = routes.ToList();

        // First pass: Add all routes as nodes
        foreach (var route in routeList)
        {
            hierarchy.AddRoute(route);
        }

        // Second pass: Establish parent-child relationships
        foreach (var route in routeList)
        {
            var parent = FindBestParent(route, routeList);
            if (parent != null && parent != route)
            {
                hierarchy.AddRoute(route, parent);
            }
        }

        return hierarchy;
    }

    /// <inheritdoc />
    public IList<RouteEntry> FindParentRoutes(RouteEntry route, RouteHierarchy hierarchy)
    {
        var node = hierarchy.FindNode(route);
        return node?.GetAncestors().Select(n => n.Route).Reverse().ToList() ?? new List<RouteEntry>();
    }

    /// <inheritdoc />
    public BreadcrumbItem ResolveBreadcrumbItem(RouteEntry route, Dictionary<string, object?> routeValues)
    {
        var breadcrumbMetadata = route.Component.GetCustomAttribute<BreadcrumbAttribute>();
        
        var item = new BreadcrumbItem
        {
            Label = breadcrumbMetadata?.Title ?? GetDefaultLabelFromRoute(route),
            Description = breadcrumbMetadata?.Description,
            Icon = breadcrumbMetadata?.Icon,
            IsHidden = breadcrumbMetadata?.Hidden ?? false,
            IsClickable = breadcrumbMetadata?.Clickable ?? true,
            CssClass = breadcrumbMetadata?.CssClass,
            RouteData = new Dictionary<string, object?>(routeValues)
        };

        // Resolve parameter substitutions in label and description
        item.Label = SubstituteParameters(item.Label, routeValues);
        if (!string.IsNullOrEmpty(item.Description))
        {
            item.Description = SubstituteParameters(item.Description, routeValues);
        }

        return item;
    }

    private static string CleanPath(string path)
    {
        var questionMarkIndex = path.IndexOf('?');
        var pathPart = questionMarkIndex >= 0 ? path[..questionMarkIndex] : path;
        return pathPart.Trim('/');
    }

    private RouteEntry? FindBestParent(RouteEntry route, List<RouteEntry> allRoutes)
    {
        var routeTemplate = route.Template;
        RouteEntry? bestParent = null;
        var maxMatchingSegments = 0;

        foreach (var candidateParent in allRoutes)
        {
            if (candidateParent == route) continue;

            var parentTemplate = candidateParent.Template;
            
            // Check if parent template could be a prefix of the route template
            if (parentTemplate.Length >= routeTemplate.Length) continue;

            var matchingSegments = CountMatchingPrefixSegments(parentTemplate, routeTemplate);
            
            // A parent must match all its segments and have at least one matching segment
            if (matchingSegments == parentTemplate.Length && matchingSegments > 0 && matchingSegments > maxMatchingSegments)
            {
                bestParent = candidateParent;
                maxMatchingSegments = matchingSegments;
            }
        }

        return bestParent;
    }

    private static int CountMatchingPrefixSegments(RouteSegment[] parent, RouteSegment[] child)
    {
        var matchCount = 0;
        var minLength = Math.Min(parent.Length, child.Length);

        for (int i = 0; i < minLength; i++)
        {
            var parentSeg = parent[i];
            var childSeg = child[i];

            // Literal segments must match exactly
            if (!parentSeg.IsParameter && !childSeg.IsParameter)
            {
                if (!parentSeg.Literal.Equals(childSeg.Literal, StringComparison.OrdinalIgnoreCase))
                    break;
            }
            // If parent has parameter at this position, it can match anything in child
            else if (parentSeg.IsParameter)
            {
                // Parameter segments are compatible
            }
            // If child has parameter but parent doesn't, they don't match
            else if (childSeg.IsParameter && !parentSeg.IsParameter)
            {
                break;
            }

            matchCount++;
        }

        return matchCount;
    }

    private string BuildPathForRoute(RouteEntry route, string[] allSegments, ref int consumedSegments, Dictionary<string, object?> routeValues)
    {
        var pathSegments = new List<string>();
        var templateIndex = 0;

        // Build path by matching template segments with actual path segments
        while (templateIndex < route.Template.Length && consumedSegments < allSegments.Length)
        {
            var templateSegment = route.Template[templateIndex];

            if (templateSegment.IsParameter)
            {
                // Use the actual segment value for parameters
                pathSegments.Add(allSegments[consumedSegments]);
                consumedSegments++;
            }
            else
            {
                // Use the literal value from template
                pathSegments.Add(templateSegment.Literal);
                consumedSegments++;
            }

            templateIndex++;
        }

        // Handle remaining template segments that might be optional or have defaults
        while (templateIndex < route.Template.Length)
        {
            var templateSegment = route.Template[templateIndex];
            if (!templateSegment.IsParameter)
            {
                pathSegments.Add(templateSegment.Literal);
            }
            else if (routeValues.TryGetValue(templateSegment.ParameterName, out var paramValue) && paramValue != null)
            {
                pathSegments.Add(paramValue.ToString()!);
            }
            templateIndex++;
        }

        return string.Join("/", pathSegments);
    }

    private static string GetDefaultLabelFromRoute(RouteEntry route)
    {
        // Get the last literal segment as a fallback label
        var lastLiteralSegment = route.Template.Where(s => !s.IsParameter).LastOrDefault();
        var lastLiteral = lastLiteralSegment.Literal;
        
        if (string.IsNullOrEmpty(lastLiteral))
            return "Page";

        // Convert kebab-case and snake_case to Title Case
        return ConvertToTitleCase(lastLiteral);
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

    private static string SubstituteParameters(string text, Dictionary<string, object?> routeValues)
    {
        if (string.IsNullOrEmpty(text) || routeValues.Count == 0)
            return text;

        // Replace parameter placeholders like {id} with actual values
        return Regex.Replace(text, @"\{(\w+)\}", match =>
        {
            var paramName = match.Groups[1].Value;
            return routeValues.TryGetValue(paramName, out var value) && value != null 
                ? value.ToString() ?? match.Value 
                : match.Value;
        });
    }
}