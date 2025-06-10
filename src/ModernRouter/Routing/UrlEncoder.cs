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
            result = ReplaceRouteParameter(result, kvp.Key, kvp.Value);
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

        // Basic sanitization - remove dangerous characters
        return System.Web.HttpUtility.UrlEncode(value);
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

        if (validateParameters)
        {
            ValidateRouteParameters(routeValues);
            
            // Check for missing required parameters
            ValidateRequiredParameters(template, routeValues);
            
            // Also validate route constraints
            if (!ValidateRouteConstraints(template, routeValues))
            {
                throw new ArgumentException("One or more route parameters do not match their constraints");
            }
        }

        return ProcessTemplate(template, routeValues);
    }

    private static void ValidateRouteParameters(Dictionary<string, object?> routeValues)
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

    /// <summary>
    /// Validates that all required parameters are provided
    /// </summary>
    /// <param name="template">Route template</param>
    /// <param name="routeValues">Provided route values</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing</exception>
    private static void ValidateRequiredParameters(string template, Dictionary<string, object?> routeValues)
    {
        if (string.IsNullOrEmpty(template))
            return;

        // Find all required parameters (non-optional parameters in the template)
        var requiredParameterPattern = @"\{([^}?:]+)(?::[^}]+)?\}";
        var optionalParameterPattern = @"\{([^}?:]+)(?::[^}]+)?\?\}";
        
        var requiredMatches = System.Text.RegularExpressions.Regex.Matches(template, requiredParameterPattern);
        var optionalMatches = System.Text.RegularExpressions.Regex.Matches(template, optionalParameterPattern);
        
        // Get list of optional parameter names
        var optionalParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (System.Text.RegularExpressions.Match match in optionalMatches)
        {
            optionalParams.Add(match.Groups[1].Value);
        }

        // Check each required parameter
        foreach (System.Text.RegularExpressions.Match match in requiredMatches)
        {
            var parameterName = match.Groups[1].Value;
            
            // Skip if this is actually an optional parameter
            if (optionalParams.Contains(parameterName))
                continue;
                
            // Check if parameter is provided and not null/empty
            if (!routeValues.ContainsKey(parameterName) || 
                routeValues[parameterName] == null ||
                (routeValues[parameterName] is string str && string.IsNullOrEmpty(str)))
            {
                throw new ArgumentException($"Required route parameter '{parameterName}' is missing or empty");
            }
        }
    }

    /// <summary>
    /// Validates route parameters against route constraints in the template
    /// </summary>
    /// <param name="template">Route template with constraints</param>
    /// <param name="routeValues">Parameter values to validate</param>
    /// <returns>True if all parameters are valid for their constraints</returns>
    public static bool ValidateRouteConstraints(string template, Dictionary<string, object?> routeValues)
    {
        if (string.IsNullOrEmpty(template) || routeValues.Count == 0)
            return true;

        foreach (var kvp in routeValues)
        {
            if (!ValidateParameterConstraint(template, kvp.Key, kvp.Value))
                return false;
        }

        return true;
    }

    private static bool ValidateParameterConstraint(string template, string parameterName, object? parameterValue)
    {
        // Extract constraint from template for this parameter
        var constraintPattern = $@"\{{{parameterName}:([^}}]+)\}}";
        var match = System.Text.RegularExpressions.Regex.Match(template, constraintPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        if (!match.Success)
            return true; // No constraint, parameter is valid

        var constraintType = match.Groups[1].Value.ToLowerInvariant();
        
        return constraintType switch
        {
            "int" => IsValidInteger(parameterValue),
            "long" => IsValidLong(parameterValue),
            "double" => IsValidDouble(parameterValue),
            "decimal" => IsValidDecimal(parameterValue),
            "bool" => IsValidBoolean(parameterValue),
            "guid" => IsValidGuid(parameterValue),
            _ => true // Unknown constraint type, assume valid
        };
    }

    private static bool IsValidInteger(object? value)
    {
        if (value == null) return false;
        if (value is int) return true;
        if (value is string str) return int.TryParse(str, out _);
        return false;
    }

    private static bool IsValidLong(object? value)
    {
        if (value == null) return false;
        if (value is long) return true;
        if (value is string str) return long.TryParse(str, out _);
        return false;
    }

    private static bool IsValidDouble(object? value)
    {
        if (value == null) return false;
        if (value is double) return true;
        if (value is string str) return double.TryParse(str, out _);
        return false;
    }

    private static bool IsValidDecimal(object? value)
    {
        if (value == null) return false;
        if (value is decimal) return true;
        if (value is string str) return decimal.TryParse(str, out _);
        return false;
    }

    private static bool IsValidBoolean(object? value)
    {
        if (value == null) return false;
        if (value is bool) return true;
        if (value is string str) return bool.TryParse(str, out _);
        return false;
    }

    private static bool IsValidGuid(object? value)
    {
        if (value == null) return false;
        if (value is Guid) return true;
        if (value is string str) return Guid.TryParse(str, out _);
        return false;
    }

    private static string ProcessTemplate(string template, Dictionary<string, object?> routeValues)
    {
        var result = template;
        
        foreach (var kvp in routeValues)
        {
            result = ReplaceRouteParameter(result, kvp.Key, kvp.Value);
        }

        return result;
    }

    private static string ReplaceRouteParameter(string template, string parameterName, object? parameterValue)
    {
        // Handle route constraints by looking for patterns like {id:int}, {id:string}, etc.
        var constraintPattern = $@"\{{{parameterName}(?::[^}}]+)?\}}";
        var optionalConstraintPattern = $@"\{{{parameterName}(?::[^}}]+)?\?\}}";
        
        if (parameterValue != null)
        {
            var encodedValue = parameterValue is string stringValue 
                ? EncodeRouteParameter(stringValue)
                : parameterValue.ToString() ?? string.Empty;
            
            template = System.Text.RegularExpressions.Regex.Replace(template, constraintPattern, encodedValue, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            template = System.Text.RegularExpressions.Regex.Replace(template, optionalConstraintPattern, encodedValue, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        else
        {
            // Remove optional parameters that are null
            template = System.Text.RegularExpressions.Regex.Replace(template, $@"/{optionalConstraintPattern}", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            template = System.Text.RegularExpressions.Regex.Replace(template, optionalConstraintPattern, string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return template;
    }
}