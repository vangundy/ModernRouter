using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;

namespace ModernRouter.Components;
public partial class Outlet
{
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [CascadingParameter] private List<RouteEntry>? RouteTable { get; set; }
    [CascadingParameter] private string[]? Segments { get; set; }
    [CascadingParameter] private RouteContext RouteContext { get; set; } = default!;
    [CascadingParameter(Name = "RouterErrorContent")]
    private RenderFragment<Exception>? RouterErrorContent { get; set; }

    private object? _loaderData;
    private bool _loading;
    private Exception? _loaderException;

    protected override async Task OnParametersSetAsync()
    {
        if (RouteTable is null || Segments is null) return;
        RouteContext = RouteMatcher.Match(RouteTable, string.Join('/', Segments));
        _loaderData = null;
        _loaderException = null;
        _loading = false;
        if (RouteContext?.Matched?.LoaderType is { } loaderType)
        {
            _loading = true;
            try
            {
                var loader = (IRouteDataLoader)ActivatorUtilities.CreateInstance(Services, loaderType);
                _loaderData = await loader.LoadAsync(RouteContext, Services, CancellationToken.None);
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