using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;
using ModernRouter.Services;

namespace ModernRouter.Extensions;

/// <summary>
/// Wrapper interface for NavigationManager extension methods to enable proper mocking in tests.
/// This interface wraps extension methods that cannot be mocked directly.
/// </summary>
public interface INavigationWrapper
{
    /// <summary>
    /// Gets the current navigation manager instance.
    /// </summary>
    NavigationManager NavigationManager { get; }

    /// <summary>
    /// Gets query parameters from the current URL.
    /// </summary>
    /// <returns>Parsed query parameters</returns>
    QueryParameters GetQueryParameters();

    /// <summary>
    /// Navigates to a URL with query parameters.
    /// </summary>
    /// <param name="uri">Base URI</param>
    /// <param name="queryParameters">Query parameters to append</param>
    /// <param name="forceLoad">Whether to force load</param>
    /// <param name="replace">Whether to replace the current entry</param>
    void NavigateTo(string uri, QueryParameters queryParameters, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates with a single query parameter update.
    /// </summary>
    /// <param name="key">Query parameter key</param>
    /// <param name="value">Query parameter value (null to remove)</param>
    /// <param name="forceLoad">Whether to force load</param>
    /// <param name="replace">Whether to replace the current entry</param>
    void NavigateWithQuery(string key, string? value, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates with multiple query parameter updates.
    /// </summary>
    /// <param name="parameters">Dictionary of parameters to update</param>
    /// <param name="forceLoad">Whether to force load</param>
    /// <param name="replace">Whether to replace the current entry</param>
    void NavigateWithQuery(Dictionary<string, string?> parameters, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates to a named route.
    /// </summary>
    /// <param name="routeNames">Route name service</param>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="forceLoad">Whether to force load</param>
    /// <param name="replace">Whether to replace the current entry</param>
    void NavigateToNamedRoute(IRouteNameService routeNames, string routeName, object? routeValues = null, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Tries to navigate to a named route.
    /// </summary>
    /// <param name="routeNames">Route name service</param>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="forceLoad">Whether to force load</param>
    /// <param name="replace">Whether to replace the current entry</param>
    /// <returns>True if navigation succeeded</returns>
    bool TryNavigateToNamedRoute(IRouteNameService routeNames, string routeName, object? routeValues = null, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Gets URL for a named route without navigating.
    /// </summary>
    /// <param name="routeNames">Route name service</param>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <returns>Generated URL</returns>
    string GetUrlForNamedRoute(IRouteNameService routeNames, string routeName, object? routeValues = null);
}