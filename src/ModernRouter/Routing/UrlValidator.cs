namespace ModernRouter.Routing;

public static class UrlValidator
{
    private static readonly char[] InvalidPathChars = { '<', '>', '"', '\0', '|', '*', '?' };

    public static UrlValidationResult ValidatePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return UrlValidationResult.Valid();

        var result = new UrlValidationResult();

        if (path.IndexOfAny(InvalidPathChars) >= 0)
            result.AddError("Path contains invalid characters");

        if (path.Length > 2048)
            result.AddError("Path exceeds maximum length");

        if (path.Contains("../") || path.Contains("..\\"))
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

        return ValidatePath(queryString);
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