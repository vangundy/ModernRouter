namespace ModernRouter.Routing;
public static class RouteMatcher
{
    public static RouteContext Match(IEnumerable<RouteEntry> entries, string path)
    {
        // Validate the incoming path for security issues
        var validationResult = UrlValidator.ValidatePath(path);
        if (!validationResult.IsValid)
        {
            // For security reasons, reject malformed URLs
            return new RouteContext 
            { 
                QueryParameters = new QueryParameters() 
            };
        }

        // Separate path from query string
        var questionMarkIndex = path.IndexOf('?');
        var pathPart = questionMarkIndex >= 0 ? path[..questionMarkIndex] : path;
        var queryPart = questionMarkIndex >= 0 ? path[questionMarkIndex..] : string.Empty;
        
        // Validate query string separately
        var queryValidation = UrlValidator.ValidateQueryString(queryPart);
        var queryParameters = queryValidation.IsValid 
            ? new QueryParameters(queryPart) 
            : new QueryParameters(); // Use empty if invalid
        
        var segments = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries)
                              .Select(UrlEncoder.DecodeRouteParameter)
                              .ToArray();

        foreach (var entry in entries)
        {
            // Try primary route first
            if (TryMatch(entry.Template, segments,
                    out var remaining, out var values))
            {
                return new RouteContext
                {
                    Matched = entry,
                    RemainingSegments = remaining,
                    RouteValues = values,
                    QueryParameters = queryParameters,
                    IsAliasMatch = false
                };
            }
            
            // Try aliases if primary route didn't match
            foreach (var alias in entry.Aliases)
            {
                if (TryMatch(alias.Template, segments,
                        out var aliasRemaining, out var aliasValues))
                {
                    return new RouteContext
                    {
                        Matched = entry,
                        RemainingSegments = aliasRemaining,
                        RouteValues = aliasValues,
                        QueryParameters = queryParameters,
                        IsAliasMatch = true,
                        MatchedAlias = alias
                    };
                }
            }
        }
        return new RouteContext 
        { 
            QueryParameters = queryParameters 
        }; // no match but preserve query parameters
    }

    private static bool TryMatch(
    RouteSegment[] template, string[] path,
    out string[] remaining,
    out Dictionary<string, object?> values)
    {
        remaining = [];
        values = [];
        var pIdx = 0;

        for (var tIdx = 0; tIdx < template.Length; tIdx++)
        {
            var seg = template[tIdx];

            if (seg.IsCatchAll)
            {
                return HandleCatchAllSegment(seg, path, pIdx, values, out remaining);
            }

            if (pIdx >= path.Length)
            {
                return HandlePathExhausted(seg, values);
            }

            var part = path[pIdx];

            if (!seg.IsParameter)
            {
                if (!HandleLiteralSegment(seg, part))
                    return false;
                pIdx++;
                continue;
            }

            if (!HandleParameterSegment(seg, part, values))
                return false;
                
            pIdx++;
        }

        remaining = pIdx < path.Length ? path[pIdx..] : [];
        return true;
    }

    private static bool HandleCatchAllSegment(RouteSegment seg, string[] path, int pIdx, 
        Dictionary<string, object?> values, out string[] remaining)
    {
        var joined = string.Join('/', path[pIdx..]);
        values[seg.ParameterName] = joined;
        remaining = [];
        return true;
    }

    private static bool HandlePathExhausted(RouteSegment seg, Dictionary<string, object?> values)
    {
        if (seg.IsOptional)
        {
            values[seg.ParameterName] = null;
            return true;
        }
        return false;
    }

    private static bool HandleLiteralSegment(RouteSegment seg, string part)
    {
        return part.Equals(seg.Literal, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HandleParameterSegment(RouteSegment seg, string part, Dictionary<string, object?> values)
    {
        // Validate the parameter value for security
        var validationResult = UrlValidator.ValidateRouteParameter(part, seg.ParameterName);
        if (!validationResult.IsValid)
        {
            return false; // Reject invalid parameter values
        }

        if (seg.Converter is null)
        {
            values[seg.ParameterName] = part;
            return true;
        }
        
        var (ok, val) = seg.Converter(part);
        if (!ok) return false;
        
        values[seg.ParameterName] = val;
        return true;
    }
}
