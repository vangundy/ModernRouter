using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;

namespace ModernRouter.Components;
public partial class RouteView
{
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [Parameter] public RouteEntry Entry { get; set; } = default!;
    [Parameter] public string[] RemainingSegments { get; set; } = [];
    [Parameter] public Dictionary<string, object?> RouteValues { get; set; } = [];
    [CascadingParameter(Name = "RouterErrorContent")]
    private RenderFragment<Exception>? RouterErrorContent { get; set; }

    private object? _loaderData;
    private bool _loading;
    private Exception? _loaderException;

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
                var loader = (IRouteDataLoader)ActivatorUtilities.CreateInstance(Services, loaderType);
                _loaderData = await loader.LoadAsync(
                    new RouteContext { Matched = Entry, RemainingSegments = RemainingSegments, RouteValues = RouteValues },
                    Services,
                    CancellationToken.None
                );
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