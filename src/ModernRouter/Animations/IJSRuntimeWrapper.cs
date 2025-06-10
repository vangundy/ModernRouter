using Microsoft.JSInterop;

namespace ModernRouter.Animations;

/// <summary>
/// Wrapper interface for IJSRuntime to enable proper mocking in tests.
/// This interface wraps extension methods that cannot be mocked directly.
/// </summary>
public interface IJSRuntimeWrapper
{
    /// <summary>
    /// Invokes a JavaScript function asynchronously without returning a value.
    /// </summary>
    /// <param name="identifier">The JavaScript function identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <param name="args">Arguments to pass to the JavaScript function.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeVoidAsync(string identifier, CancellationToken cancellationToken, params object[] args);

    /// <summary>
    /// Invokes a JavaScript function asynchronously without returning a value.
    /// </summary>
    /// <param name="identifier">The JavaScript function identifier.</param>
    /// <param name="args">Arguments to pass to the JavaScript function.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeVoidAsync(string identifier, params object[] args);

    /// <summary>
    /// Invokes a JavaScript function asynchronously and returns a value.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="identifier">The JavaScript function identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <param name="args">Arguments to pass to the JavaScript function.</param>
    /// <returns>A task representing the asynchronous operation with the return value.</returns>
    Task<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, params object[] args);

    /// <summary>
    /// Invokes a JavaScript function asynchronously and returns a value.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="identifier">The JavaScript function identifier.</param>
    /// <param name="args">Arguments to pass to the JavaScript function.</param>
    /// <returns>A task representing the asynchronous operation with the return value.</returns>
    Task<T> InvokeAsync<T>(string identifier, params object[] args);
}