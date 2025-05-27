namespace ModernRouter.Routing;
public readonly record struct RouteSegment
{
    public bool IsParameter { get; }
    public bool IsCatchAll { get; }
    public bool IsOptional { get; }
    public string Literal { get; }               // for literals
    public string ParameterName { get; }         // for parameters
    public Func<string, (bool success, object? value)>? Converter { get; }

    // literal
    public RouteSegment(string literal)
        => (IsParameter, IsCatchAll, IsOptional, Literal, ParameterName)
           = (false, false, false, literal, "");

    // parameter
    public RouteSegment(
        string name,
        bool catchAll,
        bool optional,
        Func<string, (bool, object?)>? converter)
        => (IsParameter, IsCatchAll, IsOptional, Literal, ParameterName, Converter)
        = (true, catchAll, optional, "", name, converter);
}
