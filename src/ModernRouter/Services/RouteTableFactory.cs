using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;
using System.Globalization;
using System.Reflection;

namespace ModernRouter.Services;
internal static class RouteTableFactory
{
    private static readonly Dictionary<string,
        Func<string, (bool, object?)>> _converters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["int"] = s => (int.TryParse(s, out var v), v),
            ["long"] = s => (long.TryParse(s, out var v), v),
            ["bool"] = s => (bool.TryParse(s, out var v), v),
            ["float"] = s => (float.TryParse(s, out var v), v),
            ["double"] = s => (double.TryParse(s, out var v), v),
            ["decimal"] = s => (decimal.TryParse(s, out var v), v),
            ["guid"] = s => (Guid.TryParse(s, out var v), v),
            ["datetime"] = s => (DateTime.TryParse(s, new CultureInfo("en-US"), out var v), v),
        };

    public static List<RouteEntry> Build(IEnumerable<Assembly> assemblies)
    {
        var list = new List<RouteEntry>();

        foreach (var asm in assemblies)
        {
            foreach (var type in asm.ExportedTypes)
            {
                foreach (RouteAttribute attr in type.GetCustomAttributes<RouteAttribute>())
                {
                    var segments = ParseTemplate(attr.Template);
                    list.Add(new RouteEntry(segments, type));
                }
            }
        }

        // longest template first
        list.Sort((a, b) => b.Template.Length.CompareTo(a.Template.Length));
        return list;
    }

    private static RouteSegment[] ParseTemplate(string template)
    {
        var parts = template.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var list = new List<RouteSegment>(parts.Length);

        foreach (var p in parts)
        {
            // literal segment
            if (!p.StartsWith('{') || !p.EndsWith('}'))
            {                                   
                list.Add(new RouteSegment(p));
                continue;
            }

            // parameter segment
            var inner = p[1..^1];               
            var isCatchAll = inner.StartsWith('*');
            if (isCatchAll) inner = inner.TrimStart('*');

            var isOptional = inner.EndsWith('?');
            if (isOptional) inner = inner[..^1];

            var colon = inner.IndexOf(':');
            var name = colon < 0 ? inner : inner[..colon];
            var cstr = colon < 0 ? null : inner[(colon + 1)..];

            _converters.TryGetValue(cstr ?? "", out var conv);
            list.Add(new RouteSegment(name, isCatchAll, isOptional, conv));
        }
        return [.. list];
    }
}
