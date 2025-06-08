namespace ModernRouter.Routing;

/// <summary>
/// Interface for navigation middleware that can intercept and control route navigation.
/// </summary>
/// <remarks>
/// Middleware implementations should:
/// 1. Check context.CancellationToken.ThrowIfCancellationRequested() before expensive operations
/// 2. Pass the cancellation token to any async operations they perform
/// 3. Allow OperationCanceledException to bubble up for proper cancellation handling
/// </remarks>
public interface INavMiddleware
{
    /// <summary>
    /// Invokes the middleware with the given navigation context.
    /// </summary>
    /// <param name="context">The navigation context containing route information and cancellation token</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <returns>A navigation result indicating how to proceed</returns>
    Task<NavResult> InvokeAsync(NavContext context, Func<Task<NavResult>> next);
}
