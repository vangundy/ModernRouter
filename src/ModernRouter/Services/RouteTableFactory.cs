﻿using Microsoft.AspNetCore.Components;
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
        var routes = assemblies
            .SelectMany(asm => asm.ExportedTypes)
            .SelectMany(type => type.GetCustomAttributes<RouteAttribute>()
                .Select(attr => new RouteEntry(ParseTemplate(attr.Template), type)
                {
                    // Store the template string for URL generation
                    TemplateString = attr.Template,
                    // Process route aliases
                    Aliases = ProcessAliases(type),
                    // Store all attributes from the component
                    Attributes = [.. type.GetCustomAttributes()]
                }))
            .ToList();

        // longest template first
        routes.Sort((a, b) => b.Template.Length.CompareTo(a.Template.Length));
        return routes;
    }

    private static IReadOnlyList<RouteAlias> ProcessAliases(Type componentType)
    {
        var aliasAttributes = componentType.GetCustomAttributes<RouteAliasAttribute>();
        if (!aliasAttributes.Any())
            return [];

        var aliases = new List<RouteAlias>();
        
        foreach (var aliasAttr in aliasAttributes)
        {
            try
            {
                var alias = new RouteAlias
                {
                    Template = ParseTemplate(aliasAttr.Template),
                    TemplateString = aliasAttr.Template,
                    RedirectToPrimary = aliasAttr.RedirectToPrimary,
                    Priority = aliasAttr.Priority
                };
                aliases.Add(alias);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the entire route building process
                Console.WriteLine($"Warning: Failed to parse route alias '{aliasAttr.Template}' for component {componentType.Name}: {ex.Message}");
            }
        }

        // Sort by priority (higher priority first)
        aliases.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        
        return aliases.AsReadOnly();
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
