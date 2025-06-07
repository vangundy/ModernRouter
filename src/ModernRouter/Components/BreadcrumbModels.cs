namespace ModernRouter.Components;

/// <summary>
/// Enhanced breadcrumb item with rich metadata
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Gets or sets the breadcrumb label
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the breadcrumb URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the active breadcrumb
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb icon
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets whether this breadcrumb is clickable
    /// </summary>
    public bool IsClickable { get; set; } = true;

    /// <summary>
    /// Gets or sets custom CSS classes
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets whether this breadcrumb is hidden
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// Gets or sets the route data associated with this breadcrumb
    /// </summary>
    public Dictionary<string, object?> RouteData { get; set; } = [];

    /// <summary>
    /// Gets or sets the original route segment
    /// </summary>
    public string? OriginalSegment { get; set; }
}

/// <summary>
/// Configuration options for breadcrumb component
/// </summary>
public class BreadcrumbOptions
{
    /// <summary>
    /// Gets or sets the separator between breadcrumb items
    /// </summary>
    public string Separator { get; set; } = "/";

    /// <summary>
    /// Gets or sets the home breadcrumb label
    /// </summary>
    public string HomeLabel { get; set; } = "Home";

    /// <summary>
    /// Gets or sets the home breadcrumb URL
    /// </summary>
    public string HomeUrl { get; set; } = "/";

    /// <summary>
    /// Gets or sets whether to include the home breadcrumb
    /// </summary>
    public bool IncludeHome { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of breadcrumbs to display
    /// </summary>
    public int? MaxItems { get; set; }

    /// <summary>
    /// Gets or sets whether to auto-collapse middle items when max is exceeded
    /// </summary>
    public bool AutoCollapse { get; set; } = true;

    /// <summary>
    /// Gets or sets the collapse indicator text
    /// </summary>
    public string CollapseIndicator { get; set; } = "...";

    /// <summary>
    /// Gets or sets custom CSS classes for the breadcrumb container
    /// </summary>
    public string? ContainerCssClass { get; set; }

    /// <summary>
    /// Gets or sets custom CSS classes for breadcrumb items
    /// </summary>
    public string? ItemCssClass { get; set; }

    /// <summary>
    /// Gets or sets custom CSS classes for the active breadcrumb
    /// </summary>
    public string? ActiveItemCssClass { get; set; }

    /// <summary>
    /// Gets or sets whether to use icons
    /// </summary>
    public bool ShowIcons { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show descriptions in tooltips
    /// </summary>
    public bool ShowDescriptions { get; set; } = true;

    /// <summary>
    /// Gets or sets the animation type for breadcrumb changes
    /// </summary>
    public BreadcrumbAnimation Animation { get; set; } = BreadcrumbAnimation.None;
}

/// <summary>
/// Animation types for breadcrumb transitions
/// </summary>
public enum BreadcrumbAnimation
{
    None,
    Fade,
    Slide,
    Scale
}

/// <summary>
/// Event arguments for breadcrumb events
/// </summary>
/// <param name="item">The breadcrumb item</param>
public class BreadcrumbEventArgs(BreadcrumbItem item) : EventArgs
{
    /// <summary>
    /// Gets the breadcrumb item that triggered the event
    /// </summary>
    public BreadcrumbItem Item { get; } = item;

    /// <summary>
    /// Gets or sets whether the event should be cancelled
    /// </summary>
    public bool Cancel { get; set; }
}