using ModernRouter.Components;
using ModernRouter.Routing;
using System.Reflection;

namespace ModernRouter.Services;

/// <summary>
/// Default implementation of IBreadcrumbService
/// </summary>
public class BreadcrumbService : IBreadcrumbService
{
    private readonly IRouteTableService _routeTableService;
    private readonly List<IBreadcrumbProvider> _providers = new();

    /// <inheritdoc />
    public event EventHandler<BreadcrumbGeneratedEventArgs>? BreadcrumbsGenerated;

    /// <summary>
    /// Initializes a new instance of the BreadcrumbService
    /// </summary>
    /// <param name="routeTableService">Route table service</param>
    public BreadcrumbService(IRouteTableService routeTableService)
    {
        _routeTableService = routeTableService ?? throw new ArgumentNullException(nameof(routeTableService));
    }

    /// <inheritdoc />
    public IList<BreadcrumbItem> GenerateBreadcrumbs(string path, BreadcrumbOptions? options = null)
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

        // Get route matches from the route table service
        var routeMatches = _routeTableService.GetBreadcrumbMatches(path);
        
        // Generate breadcrumbs using providers or default logic
        breadcrumbs.AddRange(routeMatches
            .Select(match => GenerateBreadcrumbForMatch(match, options))
            .Where(item => item != null && !item.IsHidden)!);

        // Sort by order
        breadcrumbs.Sort((a, b) => a.Order.CompareTo(b.Order));

        // Apply max items and auto-collapse if needed
        if (options.MaxItems.HasValue && breadcrumbs.Count > options.MaxItems.Value && options.AutoCollapse)
        {
            breadcrumbs = ApplyAutoCollapse(breadcrumbs, options.MaxItems.Value, options.CollapseIndicator);
        }

        // Fire event
        BreadcrumbsGenerated?.Invoke(this, new BreadcrumbGeneratedEventArgs(path, breadcrumbs, options));

        return breadcrumbs;
    }

    /// <inheritdoc />
    public void RegisterProvider(IBreadcrumbProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        _providers.Add(provider);
        _providers.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Higher priority first
    }

    /// <inheritdoc />
    public void RemoveProvider(IBreadcrumbProvider provider)
    {
        _providers.Remove(provider);
    }

    /// <inheritdoc />
    public BreadcrumbAttribute? GetBreadcrumbMetadata(RouteEntry routeEntry)
    {
        return routeEntry.Component.GetCustomAttribute<BreadcrumbAttribute>();
    }

    private BreadcrumbItem? GenerateBreadcrumbForMatch(BreadcrumbRouteMatch match, BreadcrumbOptions options)
    {
        // Try custom providers first
        foreach (var provider in _providers)
        {
            if (provider.CanHandle(match))
            {
                var result = provider.GenerateBreadcrumb(match, options);
                if (result != null)
                    return result;
            }
        }

        // Default breadcrumb generation
        return GenerateDefaultBreadcrumb(match);
    }

    private BreadcrumbItem GenerateDefaultBreadcrumb(BreadcrumbRouteMatch match)
    {
        var breadcrumb = new BreadcrumbItem
        {
            Label = match.Label,
            Url = match.Path,
            IsActive = match.IsActive,
            RouteData = match.RouteValues,
            Order = match.Path.Count(c => c == '/') // Depth-based ordering
        };

        // Apply breadcrumb metadata if available
        if (match.Route != null)
        {
            var metadata = GetBreadcrumbMetadata(match.Route);
            if (metadata != null)
            {
                breadcrumb.Label = metadata.Title;
                breadcrumb.Description = metadata.Description;
                breadcrumb.Icon = metadata.Icon;
                breadcrumb.IsHidden = metadata.Hidden;
                breadcrumb.IsClickable = metadata.Clickable;
                breadcrumb.CssClass = metadata.CssClass;
                if (metadata.Order > 0)
                    breadcrumb.Order = metadata.Order;
            }
        }

        return breadcrumb;
    }

    private static List<BreadcrumbItem> ApplyAutoCollapse(List<BreadcrumbItem> breadcrumbs, int maxItems, string collapseIndicator)
    {
        if (breadcrumbs.Count <= maxItems)
            return breadcrumbs;

        var result = new List<BreadcrumbItem>();
        
        // Always include first item (usually home)
        result.Add(breadcrumbs[0]);
        
        // Add collapse indicator
        if (breadcrumbs.Count > maxItems)
        {
            result.Add(new BreadcrumbItem
            {
                Label = collapseIndicator,
                Url = "#",
                IsClickable = false,
                CssClass = "breadcrumb-collapse"
            });
        }
        
        // Add last few items
        var remainingSlots = maxItems - 2; // -2 for home and collapse indicator
        var startIndex = Math.Max(1, breadcrumbs.Count - remainingSlots);
        
        for (int i = startIndex; i < breadcrumbs.Count; i++)
        {
            result.Add(breadcrumbs[i]);
        }
        
        return result;
    }
}