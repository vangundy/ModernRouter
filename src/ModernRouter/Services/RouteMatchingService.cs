using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Implementation of route matching operations shared between Router and Outlet components
/// </summary>
public class RouteMatchingService : IRouteMatchingService
{
    /// <summary>
    /// Matches a URL path against a collection of route entries using the centralized RouteMatcher logic
    /// </summary>
    /// <param name="routes">Available route entries to match against</param>
    /// <param name="path">URL path to match</param>
    /// <returns>Route context with match results</returns>
    public RouteContext MatchRoute(IEnumerable<RouteEntry> routes, string path)
    {
        return RouteMatcher.Match(routes, path);
    }

    /// <summary>
    /// Loads route data asynchronously if the route has a data loader
    /// This logic was previously duplicated in both Router and Outlet components
    /// </summary>
    /// <param name="context">Route context containing the matched route</param>
    /// <param name="services">Service provider for dependency injection</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Loaded route data or null if no loader</returns>
    public async Task<object?> LoadRouteDataAsync(RouteContext context, IServiceProvider services, CancellationToken cancellationToken = default)
    {
        if (context.Matched?.LoaderType is not { } loaderType)
            return null;

        try
        {
            var loader = (IRouteDataLoader)ActivatorUtilities.CreateInstance(services, loaderType);
            return await loader.LoadAsync(context, services, cancellationToken);
        }
        catch (Exception)
        {
            // Let the calling component handle the exception
            // This preserves the existing error handling behavior
            throw;
        }
    }

    /// <summary>
    /// Validates if a route context is ready for rendering
    /// </summary>
    /// <param name="context">Route context to validate</param>
    /// <returns>True if the context can be rendered</returns>
    public bool IsValidForRendering(RouteContext? context)
    {
        return context?.Matched is not null;
    }
}