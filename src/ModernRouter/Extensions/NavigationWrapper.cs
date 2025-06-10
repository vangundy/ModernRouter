using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;
using ModernRouter.Services;

namespace ModernRouter.Extensions;

/// <summary>
/// Default implementation of INavigationWrapper that delegates to NavigationManager extension methods.
/// </summary>
public class NavigationWrapper(NavigationManager navigationManager) : INavigationWrapper
{
    public NavigationManager NavigationManager { get; } = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

    public QueryParameters GetQueryParameters()
    {
        return NavigationManager.GetQueryParameters();
    }

    public void NavigateTo(string uri, QueryParameters queryParameters, bool forceLoad = false, bool replace = false)
    {
        NavigationManager.NavigateTo(uri, queryParameters, forceLoad, replace);
    }

    public void NavigateWithQuery(string key, string? value, bool forceLoad = false, bool replace = false)
    {
        NavigationManager.NavigateWithQuery(key, value, forceLoad, replace);
    }

    public void NavigateWithQuery(Dictionary<string, string?> parameters, bool forceLoad = false, bool replace = false)
    {
        NavigationManager.NavigateWithQuery(parameters, forceLoad, replace);
    }

    public void NavigateToNamedRoute(IRouteNameService routeNames, string routeName, object? routeValues = null, bool forceLoad = false, bool replace = false)
    {
        NavigationManager.NavigateToNamedRoute(routeNames, routeName, routeValues, forceLoad, replace);
    }

    public bool TryNavigateToNamedRoute(IRouteNameService routeNames, string routeName, object? routeValues = null, bool forceLoad = false, bool replace = false)
    {
        return NavigationManager.TryNavigateToNamedRoute(routeNames, routeName, routeValues, forceLoad, replace);
    }

    public string GetUrlForNamedRoute(IRouteNameService routeNames, string routeName, object? routeValues = null)
    {
        return NavigationManager.GetUrlForNamedRoute(routeNames, routeName, routeValues);
    }
}