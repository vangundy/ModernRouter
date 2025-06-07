namespace ModernRouter.Routing;

/// <summary>
/// Attribute to specify breadcrumb metadata for a route
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BreadcrumbAttribute : Attribute
{
    /// <summary>
    /// Gets the breadcrumb title
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the breadcrumb description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb icon
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets whether this breadcrumb should be hidden
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Gets or sets the order for this breadcrumb
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets custom CSS classes for this breadcrumb
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets whether this breadcrumb is clickable
    /// </summary>
    public bool Clickable { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the BreadcrumbAttribute
    /// </summary>
    /// <param name="title">The breadcrumb title</param>
    /// <exception cref="ArgumentException">Thrown when title is null or empty</exception>
    public BreadcrumbAttribute(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Breadcrumb title cannot be null or empty", nameof(title));
            
        Title = title;
    }
}