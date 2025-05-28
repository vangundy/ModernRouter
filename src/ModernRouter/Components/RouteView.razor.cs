using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Components;
public partial class RouteView
{
    [Parameter] public RouteEntry Entry { get; set; } = default!;
    [Parameter] public string[] RemainingSegments { get; set; } = [];
    [Parameter] public Dictionary<string, object?> RouteValues { get; set; } = [];
    [CascadingParameter(Name = "RouterErrorContent")]
    private RenderFragment<Exception>? RouterErrorContent { get; set; }

    private object? _loaderData;
    private bool _loading;
    private Exception? _loaderException;
    private IReadOnlyDictionary<string, object?> _parameters => RouteValues;

    protected override async Task OnParametersSetAsync()
    {
        _loaderData = null;
        _loaderException = null;
        _loading = false;
        if (Entry.LoaderType is { } loaderType)
        {
            _loading = true;
            try
            {
                var loader = (IRouteDataLoader)Activator.CreateInstance(loaderType)!;
                _loaderData = await loader.LoadAsync(new RouteContext { Matched = Entry, RemainingSegments = RemainingSegments, RouteValues = RouteValues }, null!, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _loaderException = ex;
                throw; // Re-throw to be caught by ErrorBoundary
            }
            finally
            {
                _loading = false;
            }
        }
    }
}