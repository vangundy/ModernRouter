using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using ModernRouter.Services;

namespace ModernRouter.Components;

/// <summary>
/// Enhanced breadcrumb component that uses hierarchical route analysis
/// </summary>
public partial class HierarchicalBreadcrumbs : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IBreadcrumbService BreadcrumbService { get; set; } = default!;

    /// <summary>
    /// Breadcrumb options configuration
    /// </summary>
    [Parameter] public BreadcrumbOptions Options { get; set; } = new();

    /// <summary>
    /// Custom template for rendering breadcrumb items
    /// </summary>
    [Parameter] public RenderFragment<BreadcrumbItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering separators
    /// </summary>
    [Parameter] public RenderFragment? SeparatorTemplate { get; set; }

    /// <summary>
    /// Custom template for rendering the toolbar
    /// </summary>
    [Parameter] public RenderFragment? ToolbarTemplate { get; set; }

    /// <summary>
    /// Whether to show the toolbar
    /// </summary>
    [Parameter] public bool ShowToolbar { get; set; }

    /// <summary>
    /// Whether to show hierarchy debugging information
    /// </summary>
    [Parameter] public bool ShowHierarchyInfo { get; set; }

    /// <summary>
    /// Additional CSS classes for the container
    /// </summary>
    [Parameter] public string? CssClass { get; set; }

    /// <summary>
    /// Inline styles for the container
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// Event fired when a breadcrumb item is clicked
    /// </summary>
    [Parameter] public EventCallback<BreadcrumbEventArgs> OnBreadcrumbClick { get; set; }

    /// <summary>
    /// Event fired when breadcrumbs are updated
    /// </summary>
    [Parameter] public EventCallback<IList<BreadcrumbItem>> OnBreadcrumbsChanged { get; set; }

    /// <summary>
    /// Event fired when the route hierarchy is built
    /// </summary>
    [Parameter] public EventCallback<RouteHierarchy> OnHierarchyBuilt { get; set; }

    private List<BreadcrumbItem> _breadcrumbs = [];
    private RouteHierarchy? _hierarchy;

    /// <summary>
    /// Gets the current breadcrumb items
    /// </summary>
    public IReadOnlyList<BreadcrumbItem> Items => _breadcrumbs.AsReadOnly();

    /// <summary>
    /// Gets the current route hierarchy
    /// </summary>
    public RouteHierarchy? Hierarchy => _hierarchy;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        BuildBreadcrumbs();
    }

    protected override void OnParametersSet()
    {
        BuildBreadcrumbs();
    }

    /// <summary>
    /// Manually refresh the breadcrumbs and rebuild hierarchy
    /// </summary>
    public void RefreshBreadcrumbs(bool rebuildHierarchy = false)
    {
        if (rebuildHierarchy)
        {
            _hierarchy = null;
        }
        BuildBreadcrumbs();
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Clear all breadcrumbs
    /// </summary>
    public void ClearBreadcrumbs()
    {
        _breadcrumbs.Clear();
        InvokeAsync(() => OnBreadcrumbsChanged.InvokeAsync(_breadcrumbs));
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Add a custom breadcrumb item
    /// </summary>
    /// <param name="item">The breadcrumb item to add</param>
    public void AddBreadcrumb(BreadcrumbItem item)
    {
        if (item == null) return;
        
        _breadcrumbs.Add(item);
        InvokeAsync(() => OnBreadcrumbsChanged.InvokeAsync(_breadcrumbs));
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Remove a breadcrumb item by URL
    /// </summary>
    /// <param name="url">The URL of the breadcrumb to remove</param>
    public void RemoveBreadcrumb(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        
        var itemToRemove = _breadcrumbs.FirstOrDefault(b => b.Url == url);
        if (itemToRemove != null)
        {
            _breadcrumbs.Remove(itemToRemove);
            InvokeAsync(() => OnBreadcrumbsChanged.InvokeAsync(_breadcrumbs));
            InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Get debugging information about the current hierarchy
    /// </summary>
    /// <returns>Hierarchy information</returns>
    public string GetHierarchyDebugInfo()
    {
        if (_hierarchy == null)
            return "No hierarchy available";

        var info = new List<string>
        {
            $"Root nodes: {_hierarchy.RootNodes.Count}",
            $"Total nodes: {_hierarchy.Nodes.Count}"
        };

        foreach (var rootNode in _hierarchy.RootNodes)
        {
            info.Add($"Root: {rootNode.Route.TemplateString} (depth: {rootNode.Depth})");
            AddChildrenInfo(rootNode, info, 1);
        }

        return string.Join("\n", info);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        BuildBreadcrumbs();
        InvokeAsync(StateHasChanged);
    }

    private void BuildBreadcrumbs()
    {
        try
        {
            var path = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            var newBreadcrumbs = BreadcrumbService.GenerateHierarchicalBreadcrumbs(path, Options);
            
            _breadcrumbs.Clear();
            _breadcrumbs.AddRange(newBreadcrumbs);
            
            // Update hierarchy reference and fire event if this is the first time
            if (_hierarchy == null && BreadcrumbService.CurrentHierarchy != null)
            {
                _hierarchy = BreadcrumbService.CurrentHierarchy;
                InvokeAsync(() => OnHierarchyBuilt.InvokeAsync(_hierarchy));
            }
            
            InvokeAsync(() => OnBreadcrumbsChanged.InvokeAsync(_breadcrumbs));
        }
        catch (Exception ex)
        {
            // Log error if logging service is available
            Console.WriteLine($"Error building hierarchical breadcrumbs: {ex.Message}");
        }
    }

    private async Task OnItemClick(BreadcrumbItem item)
    {
        try
        {
            var args = new BreadcrumbEventArgs(item);
            await OnBreadcrumbClick.InvokeAsync(args);
            
            if (!args.Cancel && item.IsClickable && !string.IsNullOrEmpty(item.Url) && item.Url != "#")
            {
                NavigationManager.NavigateTo(item.Url);
            }
        }
        catch (Exception ex)
        {
            // Log error if logging service is available
            Console.WriteLine($"Error handling breadcrumb click: {ex.Message}");
        }
    }

    private string GetItemCssClass(BreadcrumbItem item)
    {
        var classes = new List<string>();
        
        if (item.IsActive)
            classes.Add("active");
        if (item.IsActive && !string.IsNullOrEmpty(Options.ActiveItemCssClass))
            classes.Add(Options.ActiveItemCssClass);
        if (!string.IsNullOrEmpty(item.CssClass))
            classes.Add(item.CssClass);
        if (!item.IsClickable)
            classes.Add("not-clickable");
        
        // Add hierarchy-specific classes
        classes.Add($"breadcrumb-depth-{item.Order}");
            
        return string.Join(" ", classes);
    }

    private static void AddChildrenInfo(RouteNode node, List<string> info, int indent)
    {
        var prefix = new string(' ', indent * 2);
        foreach (var child in node.Children)
        {
            info.Add($"{prefix}- {child.Route.TemplateString} (depth: {child.Depth})");
            AddChildrenInfo(child, info, indent + 1);
        }
    }

    /// <summary>
    /// Get CSS classes for a specific breadcrumb item
    /// </summary>
    /// <param name="item">The breadcrumb item</param>
    /// <returns>Combined CSS classes</returns>
    public string GetCssClassForItem(BreadcrumbItem item)
    {
        return GetItemCssClass(item);
    }

    /// <summary>
    /// Check if the breadcrumb item should be displayed
    /// </summary>
    /// <param name="item">The breadcrumb item</param>
    /// <returns>True if the item should be displayed</returns>
    public bool ShouldDisplayItem(BreadcrumbItem item)
    {
        return item != null && !item.IsHidden;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }
}