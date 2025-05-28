namespace ModernRouter.Routing;
public readonly record struct NavResult
{
    public static readonly NavResult Continue = new();
    public static NavResult Cancel() => new() { IsCancelled = true };
    public static NavResult Redirect(string target)
        => new() { RedirectUri = target };

    public bool IsCancelled { get; init; }
    public string? RedirectUri { get; init; }
}
