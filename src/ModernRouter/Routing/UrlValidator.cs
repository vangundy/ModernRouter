using System.Web;

namespace ModernRouter.Routing;

public static class UrlValidator
{
    private static readonly char[] InvalidPathChars = { '<', '>', '"', '\0', '|', '*', '?' };
    
    private static readonly string[] DangerousProtocols = { 
        "javascript:", "data:", "vbscript:", "file:", "ftp:"
    };

    public static UrlValidationResult ValidatePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();

        // Decode URL to catch encoded malicious content
        var decodedPath = HttpUtility.UrlDecode(path);
        
        // Check for dangerous protocols (XSS prevention)
        foreach (var protocol in DangerousProtocols)
        {
            if (decodedPath.Contains(protocol, StringComparison.OrdinalIgnoreCase))
            {
                result.AddError("Path contains dangerous protocol");
                return result;
            }
        }

        if (decodedPath.IndexOfAny(InvalidPathChars) >= 0)
            result.AddError("Path contains invalid characters");

        if (decodedPath.Length > 2048)
            result.AddError("Path exceeds maximum length");

        // Check both original and decoded for traversal (defense in depth)
        if (decodedPath.Contains("../") || decodedPath.Contains("..\\") ||
            path.Contains("../") || path.Contains("..\\"))
            result.AddError("Path contains traversal attempts");

        return result;
    }

    public static UrlValidationResult ValidateRouteParameter(string? parameterValue, string? parameterName = null)
    {
        if (string.IsNullOrEmpty(parameterValue))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();

        if (parameterValue.IndexOfAny(InvalidPathChars) >= 0)
            result.AddError($"Parameter contains invalid characters");

        if (parameterValue.Length > 512)
            result.AddError($"Parameter exceeds maximum length");

        return result;
    }

    public static UrlValidationResult ValidateQueryString(string? queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();
        
        // Decode query string to catch encoded malicious content
        var decodedQuery = HttpUtility.UrlDecode(queryString);
        
        // Check for dangerous protocols in query values
        foreach (var protocol in DangerousProtocols)
        {
            if (decodedQuery.Contains(protocol, StringComparison.OrdinalIgnoreCase))
            {
                result.AddError("Query string contains dangerous protocol");
                return result;
            }
        }

        // Allow standard query string characters but check for dangerous content
        var invalidQueryChars = new char[] { '<', '>', '"', '\0' }; // More permissive than path
        if (decodedQuery.IndexOfAny(invalidQueryChars) >= 0)
            result.AddError("Query string contains invalid characters");

        if (decodedQuery.Length > 2048)
            result.AddError("Query string exceeds maximum length");

        // Check for traversal attempts in query values
        if (decodedQuery.Contains("../") || decodedQuery.Contains("..\\"))
            result.AddError("Query string contains traversal attempts");

        return result;
    }
}

public class UrlValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public static UrlValidationResult Valid() => new();

    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}