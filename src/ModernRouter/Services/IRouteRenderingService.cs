using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Service for route rendering operations shared between RouteView and Outlet components
/// </summary>
public interface IRouteRenderingService
{
    /// <summary>
    /// Creates the component parameters dictionary for rendering a route
    /// </summary>
    /// <param name="context">Route context containing the matched route</param>
    /// <param name="additionalParameters">Additional parameters to include</param>
    /// <returns>Dictionary of parameters for the DynamicComponent</returns>
    Dictionary<string, object?> CreateComponentParameters(RouteContext context, Dictionary<string, object?>? additionalParameters = null);

    /// <summary>
    /// Determines if a route requires data loading
    /// </summary>
    /// <param name="context">Route context to check</param>
    /// <returns>True if the route has a data loader</returns>
    bool RequiresDataLoading(RouteContext? context);

    /// <summary>
    /// Gets the component type to render for a route context
    /// </summary>
    /// <param name="context">Route context containing the matched route</param>
    /// <returns>Component type to render</returns>
    Type? GetComponentType(RouteContext? context);

    /// <summary>
    /// Creates cascading values for route rendering
    /// </summary>
    /// <param name="loaderData">Data loaded by route data loader</param>
    /// <param name="remainingSegments">Remaining path segments for nested routing</param>
    /// <returns>Dictionary of cascading values</returns>
    Dictionary<string, object?> CreateCascadingValues(object? loaderData, string[] remainingSegments);
}