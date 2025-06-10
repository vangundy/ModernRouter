using ModernRouter.Routing;

namespace ModernRouter.Tests;

internal static class TestHelpers
{
    public static RouteSegment CreateLiteralSegment(string literal)
    {
        return new RouteSegment(literal);
    }

    public static RouteSegment CreateParameterSegment(string parameterName, bool isCatchAll = false, bool isOptional = false, Func<string, (bool, object?)>? converter = null)
    {
        return new RouteSegment(parameterName, isCatchAll, isOptional, converter);
    }

    public static RouteSegment CreateIntParameterSegment(string parameterName)
    {
        return new RouteSegment(parameterName, false, false, (s) => int.TryParse(s, out var i) ? (true, i) : (false, null));
    }

    public static RouteAlias CreateRouteAlias(RouteSegment[] template, string templateString, bool redirectToPrimary = false, int priority = 0)
    {
        return new RouteAlias
        {
            Template = template,
            TemplateString = templateString,
            RedirectToPrimary = redirectToPrimary,
            Priority = priority
        };
    }
}