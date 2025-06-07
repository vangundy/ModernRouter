using System.Text;
using System.Web;

namespace ModernRouter.Routing;

/// <summary>
/// Utility class for URL encoding and decoding route parameters
/// </summary>
public static class UrlEncoder
{
    /// <summary>
    /// Encodes a route parameter value for safe URL usage
    /// </summary>
    /// <param name="value">The value to encode</param>
    /// <returns>URL-encoded value</returns>
    public static string EncodeRouteParameter(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return HttpUtility.UrlEncode(value);
    }

    /// <summary>
    /// Decodes a URL-encoded route parameter value
    /// </summary>
    /// <param name="encodedValue">The encoded value to decode</param>
    /// <returns>Decoded value</returns>
    public static string DecodeRouteParameter(string? encodedValue)
    {
        if (string.IsNullOrEmpty(encodedValue))
            return string.Empty;

        return HttpUtility.UrlDecode(encodedValue);
    }

    /// <summary>
    /// Encodes route parameter values in a dictionary
    /// </summary>
    /// <param name="routeValues">Dictionary of route values</param>
    /// <returns>New dictionary with encoded values</returns>
    public static Dictionary<string, object?> EncodeRouteValues(Dictionary<string, object?> routeValues)
    {
        var encoded = new Dictionary<string, object?>(routeValues.Count);
        
        foreach (var kvp in routeValues)
        {
            if (kvp.Value is string stringValue)
            {
                encoded[kvp.Key] = EncodeRouteParameter(stringValue);
            }
            else
            {
                encoded[kvp.Key] = kvp.Value;
            }
        }
        
        return encoded;
    }

    /// <summary>
    /// Decodes route parameter values in a dictionary
    /// </summary>
    /// <param name="routeValues">Dictionary of route values</param>
    /// <returns>New dictionary with decoded values</returns>
    public static Dictionary<string, object?> DecodeRouteValues(Dictionary<string, object?> routeValues)
    {
        var decoded = new Dictionary<string, object?>(routeValues.Count);
        
        foreach (var kvp in routeValues)
        {
            if (kvp.Value is string stringValue)
            {
                decoded[kvp.Key] = DecodeRouteParameter(stringValue);
            }
            else
            {
                decoded[kvp.Key] = kvp.Value;
            }
        }
        
        return decoded;
    }

    /// <summary>
    /// Builds a URL path with properly encoded route parameter values
    /// </summary>
    /// <param name="template">Route template (e.g., "/users/{id}/posts/{slug}")</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <returns>URL path with encoded parameters</returns>
    public static string BuildPath(string template, Dictionary<string, object?> routeValues)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        var result = template;
        
        foreach (var kvp in routeValues)
        {
            var placeholder = $"{{{kvp.Key}}}";
            var optionalPlaceholder = $"{{{kvp.Key}?}}";
            
            if (kvp.Value != null)
            {
                var encodedValue = kvp.Value is string stringValue 
                    ? EncodeRouteParameter(stringValue)
                    : kvp.Value.ToString();
                
                result = result.Replace(placeholder, encodedValue, StringComparison.OrdinalIgnoreCase);
                result = result.Replace(optionalPlaceholder, encodedValue, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Remove optional parameters that are null
                result = result.Replace($"/{optionalPlaceholder}", string.Empty, StringComparison.OrdinalIgnoreCase);
                result = result.Replace(optionalPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that a string is safe for use as a route parameter
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <returns>True if the value is safe for URL usage</returns>
    public static bool IsValidRouteParameter(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        var validation = UrlValidator.ValidateRouteParameter(value);
        return validation.IsValid;
    }

    /// <summary>
    /// Sanitizes a route parameter value by encoding unsafe characters
    /// </summary>
    /// <param name="value">Value to sanitize</param>
    /// <returns>Sanitized value safe for URL usage</returns>
    public static string SanitizeRouteParameter(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return UrlValidator.SanitizeRouteParameter(value);
    }

    /// <summary>
    /// Validates and builds a URL path with properly encoded route parameter values
    /// </summary>
    /// <param name="template">Route template (e.g., "/users/{id}/posts/{slug}")</param>
    /// <param name="routeValues">Route parameter values</param>
    /// <param name="validateParameters">Whether to validate parameter values for security</param>
    /// <returns>URL path with encoded parameters</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails and validateParameters is true</exception>
    public static string BuildValidatedPath(string template, Dictionary<string, object?> routeValues, bool validateParameters = true)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        // Validate route values if requested
        if (validateParameters)
        {
            foreach (var kvp in routeValues)
            {
                if (kvp.Value is string stringValue)
                {
                    var validation = UrlValidator.ValidateRouteParameter(stringValue, kvp.Key);
                    if (!validation.IsValid)
                    {
                        throw new ArgumentException($"Invalid route parameter '{kvp.Key}': {string.Join(", ", validation.Errors)}");
                    }
                }
            }
        }

        var result = template;
        
        foreach (var kvp in routeValues)
        {
            var placeholder = $"{{{kvp.Key}}}";
            var optionalPlaceholder = $"{{{kvp.Key}?}}";
            
            if (kvp.Value != null)
            {
                var encodedValue = kvp.Value is string stringValue 
                    ? EncodeRouteParameter(stringValue)
                    : kvp.Value.ToString();
                
                result = result.Replace(placeholder, encodedValue, StringComparison.OrdinalIgnoreCase);
                result = result.Replace(optionalPlaceholder, encodedValue, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Remove optional parameters that are null
                result = result.Replace($"/{optionalPlaceholder}", string.Empty, StringComparison.OrdinalIgnoreCase);
                result = result.Replace(optionalPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return result;
    }
}