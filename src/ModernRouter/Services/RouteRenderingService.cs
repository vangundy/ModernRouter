using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Implementation of route rendering operations shared between RouteView and Outlet components
/// </summary>
public class RouteRenderingService : IRouteRenderingService
{
    /// <summary>
    /// Creates the component parameters dictionary for rendering a route
    /// This logic was previously duplicated in RouteView and Outlet components
    /// </summary>
    /// <param name="context">Route context containing the matched route</param>
    /// <param name="additionalParameters">Additional parameters to include</param>
    /// <returns>Dictionary of parameters for the DynamicComponent</returns>
    public Dictionary<string, object?> CreateComponentParameters(RouteContext context, Dictionary<string, object?>? additionalParameters = null)
    {
        var parameters = new Dictionary<string, object?>();

        // Add route values as component parameters
        if (context.RouteValues != null)
        {
            foreach (var kvp in context.RouteValues)
            {
                parameters[kvp.Key] = kvp.Value;
            }
        }

        // Add any additional parameters
        if (additionalParameters != null)
        {
            foreach (var kvp in additionalParameters)
            {
                parameters[kvp.Key] = kvp.Value;
            }
        }

        return parameters;
    }

    /// <summary>
    /// Determines if a route requires data loading
    /// </summary>
    /// <param name="context">Route context to check</param>
    /// <returns>True if the route has a data loader</returns>
    public bool RequiresDataLoading(RouteContext? context)
    {
        return context?.Matched?.LoaderType is not null;
    }

    /// <summary>
    /// Gets the component type to render for a route context
    /// </summary>
    /// <param name="context">Route context containing the matched route</param>
    /// <returns>Component type to render</returns>
    public Type? GetComponentType(RouteContext? context)
    {
        return context?.Matched?.Component;
    }

    /// <summary>
    /// Creates cascading values for route rendering
    /// This standardizes the cascading value creation across components
    /// </summary>
    /// <param name="loaderData">Data loaded by route data loader</param>
    /// <param name="remainingSegments">Remaining path segments for nested routing</param>
    /// <returns>Dictionary of cascading values</returns>
    public Dictionary<string, object?> CreateCascadingValues(object? loaderData, string[] remainingSegments)
    {
        return new Dictionary<string, object?>
        {
            ["LoaderData"] = loaderData,
            ["RemainingSegments"] = remainingSegments
        };
    }
}