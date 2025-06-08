using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Service for route matching operations shared between Router and Outlet components
/// </summary>
public interface IRouteMatchingService
{
    /// <summary>
    /// Matches a URL path against a collection of route entries
    /// </summary>
    /// <param name="routes">Available route entries to match against</param>
    /// <param name="path">URL path to match</param>
    /// <returns>Route context with match results</returns>
    RouteContext MatchRoute(IEnumerable<RouteEntry> routes, string path);

    /// <summary>
    /// Loads route data asynchronously if the route has a data loader
    /// </summary>
    /// <param name="context">Route context containing the matched route</param>
    /// <param name="services">Service provider for dependency injection</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Loaded route data or null if no loader</returns>
    Task<object?> LoadRouteDataAsync(RouteContext context, IServiceProvider services, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a route context is ready for rendering
    /// </summary>
    /// <param name="context">Route context to validate</param>
    /// <returns>True if the context can be rendered</returns>
    bool IsValidForRendering(RouteContext? context);
}