namespace ModernRouter.Routing;
public static class RouteMatcher
{
    public static RouteContext Match(IEnumerable<RouteEntry> entries, string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            if (TryMatch(entry.Template, segments,
                    out var remaining, out var values))
            {
                return new RouteContext
                {
                    Matched = entry,
                    RemainingSegments = remaining,
                    RouteValues = values
                };
            }
        }
        return new RouteContext(); // no match
    }

    private static bool TryMatch(
    RouteSegment[] template, string[] path,
    out string[] remaining,
    out Dictionary<string, object?> values)
    {
        remaining = [];
        values = [];

        var pIdx = 0;                       // index into path

        for (var tIdx = 0; tIdx < template.Length; tIdx++)
        {
            var seg = template[tIdx];

            // CATCH-ALL
            if (seg.IsCatchAll)
            {
                var joined = string.Join('/', path[pIdx..]);
                values[seg.ParameterName] = joined;
                remaining = [];   // nothing left for deeper outlets
                return true;
            }

            // PATH EXHAUSTED
            if (pIdx >= path.Length)
            {
                if (seg.IsOptional)
                {   // supply null/default and keep going
                    values[seg.ParameterName] = null;
                    continue;
                }
                return false;
            }

            var part = path[pIdx];

            // LITERAL
            if (!seg.IsParameter)
            {
                if (!part.Equals(seg.Literal, StringComparison.OrdinalIgnoreCase))
                    return false;
                pIdx++;                    // advance path
                continue;
            }

            // PARAMETER (non-catch-all)
            if (seg.Converter is null)
            {   // string
                values[seg.ParameterName] = part;
            }
            else
            {   // constrained
                var (ok, val) = seg.Converter(part);
                if (!ok) return false;
                values[seg.ParameterName] = val;
            }
            pIdx++;
        }

        // whatever wasn't consumed is for the next outlet
        remaining = pIdx < path.Length ? path[pIdx..] : [];
        return true;
    }
}
