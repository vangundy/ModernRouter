using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;
using ModernRouter.Services;

namespace ModernRouter.Extensions;

public static class NavigationExtensions
{
    public static QueryParameters GetQueryParameters(this NavigationManager nav)
    {
        return QueryParameters.Parse(new Uri(nav.Uri));
    }

    public static void NavigateTo(this NavigationManager nav, string uri, QueryParameters queryParameters, bool forceLoad = false, bool replace = false)
    {
        var queryString = queryParameters.ToQueryString(includeQuestionMark: false);
        var fullUri = string.IsNullOrEmpty(queryString) ? uri : $"{uri}?{queryString}";
        nav.NavigateTo(fullUri, forceLoad, replace);
    }

    public static void NavigateWithQuery(this NavigationManager nav, string key, string? value, bool forceLoad = false, bool replace = false)
    {
        var queryParameters = nav.GetQueryParameters();
        
        if (value == null)
            queryParameters.Remove(key);
        else
            queryParameters.Set(key, value);
        
        var currentUri = nav.ToBaseRelativePath(nav.Uri);
        var questionMarkIndex = currentUri.IndexOf('?');
        var pathPart = questionMarkIndex >= 0 ? currentUri[..questionMarkIndex] : currentUri;
        
        nav.NavigateTo(pathPart, queryParameters, forceLoad, replace);
    }

    public static void NavigateWithQuery(this NavigationManager nav, Dictionary<string, string?> parameters, bool forceLoad = false, bool replace = false)
    {
        var queryParameters = nav.GetQueryParameters();
        
        foreach (var kvp in parameters)
        {
            if (kvp.Value == null)
                queryParameters.Remove(kvp.Key);
            else
                queryParameters.Set(kvp.Key, kvp.Value);
        }
        
        var currentUri = nav.ToBaseRelativePath(nav.Uri);
        var questionMarkIndex = currentUri.IndexOf('?');
        var pathPart = questionMarkIndex >= 0 ? currentUri[..questionMarkIndex] : currentUri;
        
        nav.NavigateTo(pathPart, queryParameters, forceLoad, replace);
    }

    public static void NavigateToNamedRoute(this NavigationManager nav, IRouteNameService routeNames, 
        string routeName, object? routeValues = null, bool forceLoad = false, bool replace = false)
    {
        var url = routeNames.GenerateUrl(routeName, routeValues);
        nav.NavigateTo(url, forceLoad, replace);
    }

    public static bool TryNavigateToNamedRoute(this NavigationManager nav, IRouteNameService routeNames,
        string routeName, object? routeValues = null, bool forceLoad = false, bool replace = false)
    {
        if (routeNames.TryGenerateUrl(routeName, routeValues, out var url))
        {
            try
            {
                nav.NavigateTo(url, forceLoad, replace);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public static string GetUrlForNamedRoute(this NavigationManager nav, IRouteNameService routeNames,
        string routeName, object? routeValues = null)
    {
        return routeNames.GenerateUrl(routeName, routeValues);
    }

}