using ModernRouter.Components;
using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Service for managing breadcrumb generation and customization
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Generates breadcrumbs for the specified path
    /// </summary>
    /// <param name="path">The path to generate breadcrumbs for</param>
    /// <param name="options">Breadcrumb options</param>
    /// <returns>List of breadcrumb items</returns>
    IList<BreadcrumbItem> GenerateBreadcrumbs(string path, BreadcrumbOptions? options = null);

    /// <summary>
    /// Registers a custom breadcrumb provider
    /// </summary>
    /// <param name="provider">The breadcrumb provider to register</param>
    void RegisterProvider(IBreadcrumbProvider provider);

    /// <summary>
    /// Removes a breadcrumb provider
    /// </summary>
    /// <param name="provider">The breadcrumb provider to remove</param>
    void RemoveProvider(IBreadcrumbProvider provider);

    /// <summary>
    /// Gets breadcrumb metadata for a route entry
    /// </summary>
    /// <param name="routeEntry">The route entry</param>
    /// <returns>Breadcrumb metadata if available</returns>
    BreadcrumbAttribute? GetBreadcrumbMetadata(RouteEntry routeEntry);

    /// <summary>
    /// Event fired when breadcrumbs are generated
    /// </summary>
    event EventHandler<BreadcrumbGeneratedEventArgs>? BreadcrumbsGenerated;
}

/// <summary>
/// Interface for custom breadcrumb providers
/// </summary>
public interface IBreadcrumbProvider
{
    /// <summary>
    /// Gets the priority of this provider (higher = executed first)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Determines if this provider can handle the specified route
    /// </summary>
    /// <param name="routeMatch">The route match</param>
    /// <returns>True if this provider can handle the route</returns>
    bool CanHandle(BreadcrumbRouteMatch routeMatch);

    /// <summary>
    /// Generates a breadcrumb item for the specified route match
    /// </summary>
    /// <param name="routeMatch">The route match</param>
    /// <param name="options">Breadcrumb options</param>
    /// <returns>Generated breadcrumb item</returns>
    BreadcrumbItem? GenerateBreadcrumb(BreadcrumbRouteMatch routeMatch, BreadcrumbOptions options);
}

/// <summary>
/// Event arguments for breadcrumb generation events
/// </summary>
public class BreadcrumbGeneratedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the path that breadcrumbs were generated for
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the generated breadcrumb items
    /// </summary>
    public IList<BreadcrumbItem> Breadcrumbs { get; }

    /// <summary>
    /// Gets the options used for generation
    /// </summary>
    public BreadcrumbOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the BreadcrumbGeneratedEventArgs
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="breadcrumbs">The breadcrumb items</param>
    /// <param name="options">The options</param>
    public BreadcrumbGeneratedEventArgs(string path, IList<BreadcrumbItem> breadcrumbs, BreadcrumbOptions options)
    {
        Path = path;
        Breadcrumbs = breadcrumbs;
        Options = options;
    }
}