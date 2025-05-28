using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using ModernRouter.Routing;
using System.Text;

namespace ModernRouter.Components;

public partial class Breadcrumbs
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    
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
    
    [CascadingParameter] private RouteContext? RouteContext { get; set; }
    
    private List<BreadcrumbItem> _breadcrumbs = [];

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
            
        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder currentPath = new();
        
        for (int i = 0; i < segments.Length; i++)
        {
            string segment = segments[i];
            
            // Skip segments that are likely route parameters (might need refinement)
            if (segment.StartsWith("{") && segment.EndsWith("}"))
                continue;
                
            // Build the current path 
            if (currentPath.Length > 0)
                currentPath.Append('/');
            currentPath.Append(segment);
            
            string url = $"/{currentPath}";
            string label = GetLabelForSegment(segment);
            bool isActive = i == segments.Length - 1;
            
            _breadcrumbs.Add(new BreadcrumbItem
            {
                Label = label,
                Url = url,
                IsActive = isActive,
                OriginalSegment = segment
            });
        }
        
        // If we have RouteContext with route values, enrich the breadcrumbs with parameter values
        EnrichBreadcrumbsWithRouteValues();
    }
    
    private void EnrichBreadcrumbsWithRouteValues()
    {
        if (RouteContext?.RouteValues == null || RouteContext.RouteValues.Count == 0)
            return;
            
        // Find parameters in breadcrumb labels/URLs and replace with actual values
        foreach (var breadcrumb in _breadcrumbs)
        {
            foreach (var kvp in RouteContext.RouteValues)
            {
                string paramName = kvp.Key;
                string paramValue = kvp.Value?.ToString() ?? string.Empty;
                
                // If the breadcrumb contains the parameter name
                if (breadcrumb.Label.Contains($"{{{paramName}}}"))
                {
                    breadcrumb.Label = breadcrumb.Label.Replace($"{{{paramName}}}", paramValue);
                }
                
                if (breadcrumb.Url.Contains($"{{{paramName}}}"))
                {
                    breadcrumb.Url = breadcrumb.Url.Replace($"{{{paramName}}}", paramValue);
                }
            }
        }
    }
    
    private string GetLabelForSegment(string segment)
    {
        // Check for custom label in provided dictionary
        if (CustomLabels != null && CustomLabels.TryGetValue(segment, out string? customLabel))
            return customLabel;
            
        // Otherwise format the segment name (convert-kebab-case to Title Case)
        return ToTitleCase(segment);
    }
    
    private static string ToTitleCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
            
        StringBuilder result = new();
        bool capitalizeNext = true;
        
        foreach (char c in text)
        {
            if (c == '-' || c == '_')
            {
                result.Append(' ');
                capitalizeNext = true;
            }
            else
            {
                result.Append(capitalizeNext ? char.ToUpper(c) : c);
                capitalizeNext = false;
            }
        }
        
        return result.ToString();
    }
    
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}

public class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? OriginalSegment { get; set; }
}