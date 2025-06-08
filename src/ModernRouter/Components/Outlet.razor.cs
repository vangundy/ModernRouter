using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;
using ModernRouter.Services;

namespace ModernRouter.Components;
public partial class Outlet
{
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [Inject] private IRouteMatchingService RouteMatchingService { get; set; } = default!;
    [Inject] private IRouteRenderingService RouteRenderingService { get; set; } = default!;
    [CascadingParameter] private List<RouteEntry>? RouteTable { get; set; }
    [CascadingParameter] private string[]? Segments { get; set; }
    [CascadingParameter] private RouteContext RouteContext { get; set; } = default!;
    [CascadingParameter(Name = "RouterErrorContent")]
    private RenderFragment<Exception>? RouterErrorContent { get; set; }
    [CascadingParameter(Name = "NavigationProgress")]
    private RenderFragment? NavigationProgress { get; set; }

    private object? _loaderData;
    private bool _loading;
    private Exception? _loaderException;

    protected override async Task OnParametersSetAsync()
    {
        if (RouteTable is null || Segments is null) return;
        
        // Use the shared route matching service
        RouteContext = RouteMatchingService.MatchRoute(RouteTable, string.Join('/', Segments));
        
        // Reset state
        _loaderData = null;
        _loaderException = null;
        _loading = false;
        
        // Use the shared data loading logic
        if (RouteRenderingService.RequiresDataLoading(RouteContext))
        {
            _loading = true;
            StateHasChanged(); // Trigger re-render to show progress
            try
            {
                _loaderData = await RouteMatchingService.LoadRouteDataAsync(RouteContext, Services, CancellationToken.None);
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