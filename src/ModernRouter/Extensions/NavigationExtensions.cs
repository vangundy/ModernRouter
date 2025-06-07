using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Extensions;

/// <summary>
/// Extension methods for NavigationManager to work with query parameters
/// </summary>
public static class NavigationExtensions
{
    /// <summary>
    /// Gets the query parameters from the current URI
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <returns>QueryParameters instance</returns>
    public static QueryParameters GetQueryParameters(this NavigationManager navigationManager)
    {
        return QueryParameters.Parse(new Uri(navigationManager.Uri));
    }

    /// <summary>
    /// Navigates to the specified URI with query parameters
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="uri">Base URI to navigate to</param>
    /// <param name="queryParameters">Query parameters to append</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    /// <exception cref="ArgumentException">Thrown when URI is invalid or unsafe</exception>
    public static void NavigateTo(this NavigationManager navigationManager, string uri, QueryParameters queryParameters, bool forceLoad = false, bool replace = false)
    {
        // Validate the base URI
        var uriValidation = UrlValidator.ValidatePath(uri);
        if (!uriValidation.IsValid)
        {
            throw new ArgumentException($"Invalid or unsafe URI: {string.Join(", ", uriValidation.Errors)}", nameof(uri));
        }

        var queryString = queryParameters.ToQueryString(includeQuestionMark: false);
        
        // Validate the complete query string
        if (!string.IsNullOrEmpty(queryString))
        {
            var queryValidation = UrlValidator.ValidateQueryString(queryString);
            if (!queryValidation.IsValid)
            {
                throw new ArgumentException($"Invalid or unsafe query parameters: {string.Join(", ", queryValidation.Errors)}", nameof(queryParameters));
            }
        }
        
        var fullUri = string.IsNullOrEmpty(queryString) ? uri : $"{uri}?{queryString}";
        navigationManager.NavigateTo(fullUri, forceLoad, replace);
    }

    /// <summary>
    /// Navigates to the specified route with route parameters and query parameters
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="routeTemplate">Route template (e.g., "/users/{id}/posts/{slug}")</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="queryParameters">Query parameters to append</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    /// <exception cref="ArgumentException">Thrown when route template or parameters are invalid or unsafe</exception>
    public static void NavigateToRoute(this NavigationManager navigationManager, string routeTemplate, 
        Dictionary<string, object?> routeValues, QueryParameters? queryParameters = null, 
        bool forceLoad = false, bool replace = false)
    {
        // Validate route template
        var templateValidation = UrlValidator.ValidatePath(routeTemplate);
        if (!templateValidation.IsValid)
        {
            throw new ArgumentException($"Invalid route template: {string.Join(", ", templateValidation.Errors)}", nameof(routeTemplate));
        }

        // Validate route parameter values
        foreach (var kvp in routeValues)
        {
            if (kvp.Value is string stringValue)
            {
                var paramValidation = UrlValidator.ValidateRouteParameter(stringValue, kvp.Key);
                if (!paramValidation.IsValid)
                {
                    throw new ArgumentException($"Invalid route parameter '{kvp.Key}': {string.Join(", ", paramValidation.Errors)}", nameof(routeValues));
                }
            }
        }

        var path = UrlEncoder.BuildPath(routeTemplate, routeValues);
        
        if (queryParameters != null && queryParameters.Count > 0)
        {
            navigationManager.NavigateTo(path, queryParameters, forceLoad, replace);
        }
        else
        {
            // Still validate the generated path
            var pathValidation = UrlValidator.ValidatePath(path);
            if (!pathValidation.IsValid)
            {
                throw new ArgumentException($"Generated path is invalid: {string.Join(", ", pathValidation.Errors)}");
            }
            
            navigationManager.NavigateTo(path, forceLoad, replace);
        }
    }

    /// <summary>
    /// Navigates to the specified route with route parameters
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="routeTemplate">Route template (e.g., "/users/{id}")</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void NavigateToRoute(this NavigationManager navigationManager, string routeTemplate, 
        Dictionary<string, object?> routeValues, bool forceLoad = false, bool replace = false)
    {
        NavigateToRoute(navigationManager, routeTemplate, routeValues, null, forceLoad, replace);
    }

    /// <summary>
    /// Navigates to the current URI with updated query parameters
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="queryParameters">Query parameters to set</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void NavigateWithQuery(this NavigationManager navigationManager, QueryParameters queryParameters, bool forceLoad = false, bool replace = false)
    {
        var currentUri = navigationManager.ToBaseRelativePath(navigationManager.Uri);
        var questionMarkIndex = currentUri.IndexOf('?');
        var pathPart = questionMarkIndex >= 0 ? currentUri[..questionMarkIndex] : currentUri;
        
        navigationManager.NavigateTo(pathPart, queryParameters, forceLoad, replace);
    }

    /// <summary>
    /// Navigates to the current URI with a single query parameter added or updated
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="key">Parameter key</param>
    /// <param name="value">Parameter value</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void NavigateWithQuery(this NavigationManager navigationManager, string key, string? value, bool forceLoad = false, bool replace = false)
    {
        var queryParameters = navigationManager.GetQueryParameters();
        
        if (value == null)
        {
            queryParameters.Remove(key);
        }
        else
        {
            queryParameters.Set(key, value);
        }
        
        navigationManager.NavigateWithQuery(queryParameters, forceLoad, replace);
    }

    /// <summary>
    /// Navigates to the current URI with multiple query parameters added or updated
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="parameters">Dictionary of parameters to set</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void NavigateWithQuery(this NavigationManager navigationManager, Dictionary<string, string?> parameters, bool forceLoad = false, bool replace = false)
    {
        var queryParameters = navigationManager.GetQueryParameters();
        
        foreach (var kvp in parameters)
        {
            if (kvp.Value == null)
            {
                queryParameters.Remove(kvp.Key);
            }
            else
            {
                queryParameters.Set(kvp.Key, kvp.Value);
            }
        }
        
        navigationManager.NavigateWithQuery(queryParameters, forceLoad, replace);
    }

    /// <summary>
    /// Removes a query parameter from the current URI and navigates
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="key">Parameter key to remove</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void RemoveQueryParameter(this NavigationManager navigationManager, string key, bool forceLoad = false, bool replace = false)
    {
        navigationManager.NavigateWithQuery(key, null, forceLoad, replace);
    }

    /// <summary>
    /// Removes multiple query parameters from the current URI and navigates
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="keys">Parameter keys to remove</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void RemoveQueryParameters(this NavigationManager navigationManager, IEnumerable<string> keys, bool forceLoad = false, bool replace = false)
    {
        var queryParameters = navigationManager.GetQueryParameters();
        
        foreach (var key in keys)
        {
            queryParameters.Remove(key);
        }
        
        navigationManager.NavigateWithQuery(queryParameters, forceLoad, replace);
    }

    /// <summary>
    /// Clears all query parameters from the current URI and navigates
    /// </summary>
    /// <param name="navigationManager">Navigation manager instance</param>
    /// <param name="forceLoad">Whether to force load the page</param>
    /// <param name="replace">Whether to replace the current entry in history</param>
    public static void ClearQueryParameters(this NavigationManager navigationManager, bool forceLoad = false, bool replace = false)
    {
        navigationManager.NavigateWithQuery(new QueryParameters(), forceLoad, replace);
    }
}