using System.Text.RegularExpressions;

namespace ModernRouter.Routing;

/// <summary>
/// Comprehensive URL validation and sanitization utilities
/// </summary>
public static class UrlValidator
{

    private static readonly Regex MaliciousPatterns = new(
        @"(\.\./|\.\.\\|javascript:|data:|vbscript:|file:|ftp:|mailto:|tel:|callto:)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex SqlInjectionPatterns = new(
        @"(\bunion\b|\bselect\b|\binsert\b|\bupdate\b|\bdelete\b|\bdrop\b|\bcreate\b|\balter\b|\bexec\b|\bscript\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex XssPatterns = new(
        @"(<script|<iframe|<object|<embed|<link|<meta|javascript:|data:text/html|vbscript:|onload=|onerror=|onclick=|onmouseover=)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly char[] DangerousChars = { '<', '>', '"', '\'', '&', '\0', '\r', '\n', '\t' };
    private static readonly char[] InvalidPathChars = { '|', '*', '?', '"', '<', '>', '\0' };
    
    private const string UnknownParameter = "unknown";

    /// <summary>
    /// Validates if a URL path is safe and well-formed
    /// </summary>
    /// <param name="path">URL path to validate</param>
    /// <returns>Validation result with details</returns>
    public static UrlValidationResult ValidatePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();

        // Check for null characters and control characters
        if (ContainsNullOrControlChars(path))
        {
            result.AddError("Path contains null or control characters");
        }

        // Check for invalid path characters
        if (path.IndexOfAny(InvalidPathChars) >= 0)
        {
            result.AddError("Path contains invalid characters");
        }

        // Check path length
        if (path.Length > 2048)
        {
            result.AddError("Path exceeds maximum length (2048 characters)");
        }

        // Check for malicious patterns
        if (MaliciousPatterns.IsMatch(path))
        {
            result.AddError("Path contains potentially malicious patterns");
        }

        // Check for XSS attempts
        if (XssPatterns.IsMatch(path))
        {
            result.AddError("Path contains potential XSS patterns");
        }

        // Check for SQL injection attempts
        if (SqlInjectionPatterns.IsMatch(path))
        {
            result.AddWarning("Path contains potential SQL injection patterns");
        }

        // Check for excessive URL encoding
        if (HasExcessiveUrlEncoding(path))
        {
            result.AddWarning("Path has excessive URL encoding");
        }

        // Check for valid URI format
        if (!IsValidUriFormat(path))
        {
            result.AddError("Path is not a valid URI format");
        }

        // Check for path traversal attempts
        if (HasPathTraversalAttempts(path))
        {
            result.AddError("Path contains path traversal attempts");
        }

        return result;
    }

    /// <summary>
    /// Validates if a route parameter value is safe
    /// </summary>
    /// <param name="parameterValue">Parameter value to validate</param>
    /// <param name="parameterName">Name of the parameter (for context)</param>
    /// <returns>Validation result</returns>
    public static UrlValidationResult ValidateRouteParameter(string? parameterValue, string? parameterName = null)
    {
        if (string.IsNullOrEmpty(parameterValue))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();

        var paramName = parameterName ?? UnknownParameter;
        
        // Check for dangerous characters
        if (parameterValue.IndexOfAny(DangerousChars) >= 0)
        {
            result.AddError($"Parameter '{paramName}' contains dangerous characters");
        }

        // Check parameter length
        if (parameterValue.Length > 512)
        {
            result.AddError($"Parameter '{paramName}' exceeds maximum length (512 characters)");
        }

        // Check for XSS attempts
        if (XssPatterns.IsMatch(parameterValue))
        {
            result.AddError($"Parameter '{paramName}' contains potential XSS patterns");
        }

        // Check for SQL injection attempts
        if (SqlInjectionPatterns.IsMatch(parameterValue))
        {
            result.AddWarning($"Parameter '{paramName}' contains potential SQL injection patterns");
        }

        return result;
    }

    /// <summary>
    /// Validates query string parameters
    /// </summary>
    /// <param name="queryString">Query string to validate</param>
    /// <returns>Validation result</returns>
    public static UrlValidationResult ValidateQueryString(string? queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();

        // Remove leading '?' if present
        var query = queryString.StartsWith('?') ? queryString[1..] : queryString;

        // Check overall query string length
        if (query.Length > 2048)
        {
            result.AddError("Query string exceeds maximum length (2048 characters)");
        }

        // Parse and validate individual parameters
        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        
        if (pairs.Length > 100)
        {
            result.AddWarning("Query string has excessive number of parameters (>100)");
        }

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = parts.Length > 0 ? parts[0] : string.Empty;
            var value = parts.Length > 1 ? parts[1] : string.Empty;

            // Validate parameter key
            if (string.IsNullOrEmpty(key))
            {
                result.AddWarning("Query string contains parameter with empty key");
                continue;
            }

            // Validate parameter value
            var paramResult = ValidateRouteParameter(value, key);
            result.Merge(paramResult);
        }

        return result;
    }

    /// <summary>
    /// Sanitizes a URL path by removing or encoding dangerous elements
    /// </summary>
    /// <param name="path">Path to sanitize</param>
    /// <returns>Sanitized path</returns>
    public static string SanitizePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Remove null and control characters
        var sanitized = RemoveNullAndControlChars(path);

        // Remove or replace dangerous patterns
        sanitized = MaliciousPatterns.Replace(sanitized, string.Empty);

        // URL encode if needed
        if (ContainsDangerousChars(sanitized))
        {
            sanitized = UrlEncoder.EncodeRouteParameter(sanitized);
        }

        // Limit length
        if (sanitized.Length > 2048)
        {
            sanitized = sanitized[..2048];
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes a route parameter value
    /// </summary>
    /// <param name="parameterValue">Parameter value to sanitize</param>
    /// <returns>Sanitized parameter value</returns>
    public static string SanitizeRouteParameter(string? parameterValue)
    {
        if (string.IsNullOrEmpty(parameterValue))
            return string.Empty;

        // Remove dangerous characters
        var sanitized = RemoveNullAndControlChars(parameterValue);
        sanitized = RemoveDangerousChars(sanitized);

        // Remove XSS patterns
        sanitized = XssPatterns.Replace(sanitized, string.Empty);

        // Limit length
        if (sanitized.Length > 512)
        {
            sanitized = sanitized[..512];
        }

        return sanitized;
    }

    /// <summary>
    /// Checks if a string is a valid absolute or relative URI
    /// </summary>
    /// <param name="uriString">URI string to validate</param>
    /// <returns>True if valid URI format</returns>
    public static bool IsValidUri(string? uriString)
    {
        if (string.IsNullOrEmpty(uriString))
            return false;

        return Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out _);
    }

    private static bool ContainsNullOrControlChars(string input)
    {
        return input.Any(c => char.IsControl(c) || c == '\0');
    }

    private static bool ContainsDangerousChars(string input)
    {
        return input.IndexOfAny(DangerousChars) >= 0;
    }

    private static bool HasExcessiveUrlEncoding(string input)
    {
        // Check if more than 50% of the string is URL encoded
        var encodedCount = 0;
        var i = 0;
        
        while (i < input.Length - 2)
        {
            if (input[i] == '%' && 
                i + 2 < input.Length && 
                Uri.IsHexDigit(input[i + 1]) && 
                Uri.IsHexDigit(input[i + 2]))
            {
                encodedCount += 3;
                i += 3; // Skip the percent sign and hex digits
            }
            else
            {
                i++;
            }
        }
        
        return encodedCount > input.Length * 0.5;
    }

    private static bool IsValidUriFormat(string path)
    {
        try
        {
            // Try to create a URI to validate format
            _ = new Uri(path, UriKind.RelativeOrAbsolute);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HasPathTraversalAttempts(string path)
    {
        return path.Contains("../") || 
               path.Contains("..\\") || 
               path.Contains("%2e%2e%2f") || 
               path.Contains("%2e%2e%5c");
    }

    private static string RemoveNullAndControlChars(string input)
    {
        return new string(input.Where(c => !char.IsControl(c) && c != '\0').ToArray());
    }

    private static string RemoveDangerousChars(string input)
    {
        return new string(input.Where(c => !DangerousChars.Contains(c)).ToArray());
    }
}

/// <summary>
/// Result of URL validation with errors and warnings
/// </summary>
public class UrlValidationResult
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    /// <summary>
    /// Gets whether the validation passed (no errors)
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Gets whether there are any warnings
    /// </summary>
    public bool HasWarnings => _warnings.Count > 0;

    /// <summary>
    /// Gets all validation errors
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Gets all validation warnings
    /// </summary>
    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

    /// <summary>
    /// Adds an error to the validation result
    /// </summary>
    /// <param name="error">Error message</param>
    public void AddError(string error)
    {
        _errors.Add(error);
    }

    /// <summary>
    /// Adds a warning to the validation result
    /// </summary>
    /// <param name="warning">Warning message</param>
    public void AddWarning(string warning)
    {
        _warnings.Add(warning);
    }

    /// <summary>
    /// Merges another validation result into this one
    /// </summary>
    /// <param name="other">Other validation result to merge</param>
    public void Merge(UrlValidationResult other)
    {
        _errors.AddRange(other._errors);
        _warnings.AddRange(other._warnings);
    }

    /// <summary>
    /// Creates a valid result
    /// </summary>
    /// <returns>Valid validation result</returns>
    public static UrlValidationResult Valid()
    {
        return new UrlValidationResult();
    }

    /// <summary>
    /// Creates an invalid result with the specified error
    /// </summary>
    /// <param name="error">Error message</param>
    /// <returns>Invalid validation result</returns>
    public static UrlValidationResult Invalid(string error)
    {
        var result = new UrlValidationResult();
        result.AddError(error);
        return result;
    }

    /// <summary>
    /// Gets a summary of all errors and warnings
    /// </summary>
    /// <returns>Summary string</returns>
    public override string ToString()
    {
        var messages = new List<string>();
        
        if (_errors.Count > 0)
        {
            messages.Add($"Errors: {string.Join(", ", _errors)}");
        }
        
        if (_warnings.Count > 0)
        {
            messages.Add($"Warnings: {string.Join(", ", _warnings)}");
        }
        
        return messages.Count > 0 ? string.Join("; ", messages) : "Valid";
    }
}