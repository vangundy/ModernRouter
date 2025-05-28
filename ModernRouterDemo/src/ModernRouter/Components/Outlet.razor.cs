using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;
using System.Threading;
using System.Threading.Tasks;

namespace ModernRouter.Components;
public partial class Outlet
{
    [CascadingParameter] private List<RouteEntry>? RouteTable { get; set; }
    [CascadingParameter] private string[]? Segments { get; set; }
    [CascadingParameter] private RouteContext RouteContext { get; set; } = default!;

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
                var loader = (IRouteDataLoader)Activator.CreateInstance(loaderType)!;
                _loaderData = await loader.LoadAsync(RouteContext, null!, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _loaderException = ex;
            }
            finally
            {
                _loading = false;
            }
        }
    }
}