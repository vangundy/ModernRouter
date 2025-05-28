namespace ModernRouter.Routing;

public enum NavResultType
{
    Allow,
    Redirect,
    Cancel,
    Error
}

public class NavResult
{
    public NavResultType Type { get; }
    public string? RedirectUrl { get; init; }
    public Exception? Exception { get; init; }
    
    public NavResult(NavResultType type) => Type = type;
    
    public static NavResult Allow() => new(NavResultType.Allow);
    public static NavResult Redirect(string url) => new(NavResultType.Redirect) { RedirectUrl = url };
    public static NavResult Cancel() => new(NavResultType.Cancel);
    public static NavResult Error(Exception ex) => new(NavResultType.Error) { Exception = ex };
}
