using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using ModernRouter.Routing;
using ModernRouter.Services;

namespace ModernRouter.Components;

public partial class Breadcrumbs : IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IRouteTableService RouteTableService { get; set; } = default!;
    
    [Parameter] public string Separator { get; set; } = "/";
    [Parameter] public string HomeLabel { get; set; } = "Home";
    [Parameter] public string HomeUrl { get; set; } = "/";
    [Parameter] public RenderFragment<BreadcrumbItem>? ItemTemplate { get; set; }
    [Parameter] public bool IncludeHome { get; set; } = true;
    [Parameter] public Dictionary<string, string>? CustomLabels { get; set; }
    
    /// <summary>
    /// Class applied to the breadcrumbs container
    /// </summary>
    [Parameter] public string? CssClass { get; set; }
    
    private readonly List<BreadcrumbItem> _breadcrumbs = [];

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    
    protected override void OnParametersSet()
    {
        BuildBreadcrumbs();
    }
    
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        BuildBreadcrumbs();
        StateHasChanged();
    }
    
    private void BuildBreadcrumbs()
    {
        _breadcrumbs.Clear();
        
        // Add home breadcrumb if configured
        if (IncludeHome)
        {
            _breadcrumbs.Add(new BreadcrumbItem
            {
                Label = HomeLabel,
                Url = HomeUrl,
                IsActive = false
            });
        }

        string path = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        if (string.IsNullOrEmpty(path))
            return;
            
        // Use the route table service for proper breadcrumb matching
        var breadcrumbMatches = RouteTableService.GetBreadcrumbMatches(path);
        
        foreach (var match in breadcrumbMatches)
        {
            var label = GetLabelForMatch(match);
            
            _breadcrumbs.Add(new BreadcrumbItem
            {
                Label = label,
                Url = match.Path,
                IsActive = match.IsActive,
                OriginalSegment = match.Path.Split('/').LastOrDefault() ?? string.Empty
            });
        }
    }
    
    private string GetLabelForMatch(BreadcrumbRouteMatch match)
    {
        // Check for custom label first
        if (CustomLabels != null)
        {
            var segment = match.Path.Split('/').LastOrDefault() ?? string.Empty;
            if (CustomLabels.TryGetValue(segment, out string? customLabel))
                return customLabel;
        }
        
        // If this is a parameter, use the actual value if available
        if (match.IsParameter && !string.IsNullOrEmpty(match.Label))
        {
            return match.Label;
        }
        
        // Use the default label from the match
        return string.IsNullOrEmpty(match.Label) ? "Unknown" : match.Label;
    }

    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? OriginalSegment { get; set; }
}