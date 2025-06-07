using ModernRouter.Routing;

namespace ModernRouter.Services;

/// <summary>
/// Service for managing named routes and generating URLs from route names
/// </summary>
public interface IRouteNameService
{
    /// <summary>
    /// Registers a named route
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeEntry">Route entry to register</param>
    void RegisterRoute(string routeName, RouteEntry routeEntry);

    /// <summary>
    /// Gets a route entry by name
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <returns>Route entry if found, null otherwise</returns>
    RouteEntry? GetRoute(string routeName);

    /// <summary>
    /// Generates a URL for a named route with parameters
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="validateParameters">Whether to validate parameters for security</param>
    /// <returns>Generated URL</returns>
    /// <exception cref="ArgumentException">Thrown when route name is not found or parameters are invalid</exception>
    string GenerateUrl(string routeName, object? routeValues = null, bool validateParameters = true);

    /// <summary>
    /// Generates a URL for a named route with parameters
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="validateParameters">Whether to validate parameters for security</param>
    /// <returns>Generated URL</returns>
    /// <exception cref="ArgumentException">Thrown when route name is not found or parameters are invalid</exception>
    string GenerateUrl(string routeName, Dictionary<string, object?> routeValues, bool validateParameters = true);

    /// <summary>
    /// Tries to generate a URL for a named route
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="url">Generated URL if successful</param>
    /// <param name="validateParameters">Whether to validate parameters for security</param>
    /// <returns>True if URL was generated successfully</returns>
    bool TryGenerateUrl(string routeName, object? routeValues, out string url, bool validateParameters = true);

    /// <summary>
    /// Tries to generate a URL for a named route
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="url">Generated URL if successful</param>
    /// <param name="validateParameters">Whether to validate parameters for security</param>
    /// <returns>True if URL was generated successfully</returns>
    bool TryGenerateUrl(string routeName, Dictionary<string, object?> routeValues, out string url, bool validateParameters = true);

    /// <summary>
    /// Checks if a route with the specified name exists
    /// </summary>
    /// <param name="routeName">Name of the route</param>
    /// <returns>True if route exists</returns>
    bool HasRoute(string routeName);

    /// <summary>
    /// Gets all registered route names
    /// </summary>
    /// <returns>Collection of route names</returns>
    IEnumerable<string> GetRouteNames();

    /// <summary>
    /// Clears all registered routes
    /// </summary>
    void Clear();
}