using ModernRouter.Routing;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModernRouter.Services;

/// <summary>
/// Default implementation of IRouteNameService
/// </summary>
public class RouteNameService : IRouteNameService
{
    private readonly ConcurrentDictionary<string, RouteEntry> _namedRoutes = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void RegisterRoute(string routeName, RouteEntry routeEntry)
    {
        if (string.IsNullOrWhiteSpace(routeName))
            throw new ArgumentException("Route name cannot be null or empty", nameof(routeName));
        
        ArgumentNullException.ThrowIfNull(routeEntry);

        _namedRoutes.AddOrUpdate(routeName, routeEntry, (_, _) => routeEntry);
    }

    /// <inheritdoc />
    public RouteEntry? GetRoute(string routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName))
            return null;

        _namedRoutes.TryGetValue(routeName, out var route);
        return route;
    }

    /// <inheritdoc />
    public string GenerateUrl(string routeName, object? routeValues = null, bool validateParameters = true)
    {
        var parameters = ConvertToStringDictionary(routeValues);
        return GenerateUrl(routeName, parameters, validateParameters);
    }

    /// <inheritdoc />
    public string GenerateUrl(string routeName, Dictionary<string, object?> routeValues, bool validateParameters = true)
    {
        if (!TryGenerateUrl(routeName, routeValues, out var url, validateParameters))
        {
            throw new ArgumentException($"Could not generate URL for route '{routeName}'", nameof(routeName));
        }

        return url;
    }

    /// <inheritdoc />
    public bool TryGenerateUrl(string routeName, object? routeValues, out string url, bool validateParameters = true)
    {
        var parameters = ConvertToStringDictionary(routeValues);
        return TryGenerateUrl(routeName, parameters, out url, validateParameters);
    }

    /// <inheritdoc />
    public bool TryGenerateUrl(string routeName, Dictionary<string, object?> routeValues, out string url, bool validateParameters = true)
    {
        url = string.Empty;

        if (string.IsNullOrWhiteSpace(routeName))
            return false;

        var route = GetRoute(routeName);
        if (route == null)
            return false;

        try
        {
            // Check route constraints first if validation is enabled
            if (validateParameters && !UrlEncoder.ValidateRouteConstraints(route.TemplateString, routeValues))
            {
                return false;
            }
            
            url = validateParameters 
                ? UrlEncoder.BuildValidatedPath(route.TemplateString, routeValues, validateParameters)
                : UrlEncoder.BuildPath(route.TemplateString, routeValues);
            
            return !string.IsNullOrEmpty(url);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool HasRoute(string routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName))
            return false;

        return _namedRoutes.ContainsKey(routeName);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetRouteNames()
    {
        return [.. _namedRoutes.Keys];
    }

    /// <inheritdoc />
    public void Clear()
    {
        _namedRoutes.Clear();
    }

    /// <summary>
    /// Converts an anonymous object or dictionary to a string dictionary
    /// </summary>
    /// <param name="routeValues">Object to convert</param>
    /// <returns>String dictionary</returns>
    private static Dictionary<string, object?> ConvertToStringDictionary(object? routeValues)
    {
        if (routeValues == null)
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (routeValues is Dictionary<string, object?> dict)
        {
            // Ensure the dictionary uses case-insensitive comparison
            if (dict.Comparer == StringComparer.OrdinalIgnoreCase)
                return dict;
            
            // Create a new case-insensitive dictionary with the same values
            var caseInsensitiveDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in dict)
            {
                caseInsensitiveDict[kvp.Key] = kvp.Value;
            }
            return caseInsensitiveDict;
        }

        // Handle anonymous objects
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var properties = routeValues.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var prop in properties)
        {
            var value = prop.GetValue(routeValues);
            result[prop.Name] = value;
        }

        return result;
    }
}