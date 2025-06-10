using Microsoft.JSInterop;

namespace ModernRouter.Animations;

/// <summary>
/// Default implementation of IJSRuntimeWrapper that wraps the actual IJSRuntime.
/// </summary>
internal class JSRuntimeWrapper : IJSRuntimeWrapper
{
    private readonly IJSRuntime _jsRuntime;

    public JSRuntimeWrapper(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task InvokeVoidAsync(string identifier, CancellationToken cancellationToken, params object[] args)
    {
        await _jsRuntime.InvokeVoidAsync(identifier, cancellationToken, args);
    }

    public async Task InvokeVoidAsync(string identifier, params object[] args)
    {
        await _jsRuntime.InvokeVoidAsync(identifier, args);
    }

    public async Task<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, params object[] args)
    {
        return await _jsRuntime.InvokeAsync<T>(identifier, cancellationToken, args);
    }

    public async Task<T> InvokeAsync<T>(string identifier, params object[] args)
    {
        return await _jsRuntime.InvokeAsync<T>(identifier, args);
    }
}