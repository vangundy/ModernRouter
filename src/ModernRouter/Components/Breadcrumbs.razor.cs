using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using ModernRouter.Services;

namespace ModernRouter.Components;

/// <summary>
/// Code-behind for Breadcrumbs component
/// </summary>
public partial class Breadcrumbs : ComponentBase, IDisposable
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

    private List<BreadcrumbItem> _breadcrumbs = new();

    /// <summary>
    /// Gets the current breadcrumb items
    /// </summary>
    public IReadOnlyList<BreadcrumbItem> Items => _breadcrumbs.AsReadOnly();

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
    /// Manually refresh the breadcrumbs
    /// </summary>
    public void RefreshBreadcrumbs()
    {
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
            var newBreadcrumbs = BreadcrumbService.GenerateBreadcrumbs(path, Options);
            
            _breadcrumbs.Clear();
            _breadcrumbs.AddRange(newBreadcrumbs);
            
            InvokeAsync(() => OnBreadcrumbsChanged.InvokeAsync(_breadcrumbs));
        }
        catch (Exception ex)
        {
            // Log error if logging service is available
            Console.WriteLine($"Error building breadcrumbs: {ex.Message}");
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
            
        return string.Join(" ", classes);
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