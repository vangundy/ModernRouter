using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;
using ModernRouter.Services;

namespace ModernRouter.Components;
public partial class RouteView
{
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [Inject] private IRouteMatchingService RouteMatchingService { get; set; } = default!;
    [Inject] private IRouteRenderingService RouteRenderingService { get; set; } = default!;
    [Parameter] public RouteEntry RouteEntry { get; set; } = default!;
    [Parameter] public string[] RemainingSegments { get; set; } = [];
    [Parameter] public Dictionary<string, object?> RouteValues { get; set; } = [];
    [CascadingParameter(Name = "RouterErrorContent")]
    private RenderFragment<Exception>? RouterErrorContent { get; set; }

    private object? _loaderData;
    private bool _loading;
    private Exception? _loaderException;

    protected override async Task OnParametersSetAsync()
    {
        // Reset state
        _loaderData = null;
        _loaderException = null;
        _loading = false;
        
        // Create route context from parameters
        var routeContext = new RouteContext 
        { 
            Matched = RouteEntry, 
            RemainingSegments = RemainingSegments, 
            RouteValues = RouteValues 
        };
        
        // Use the shared data loading logic
        if (RouteRenderingService.RequiresDataLoading(routeContext))
        {
            _loading = true;
            try
            {
                _loaderData = await RouteMatchingService.LoadRouteDataAsync(routeContext, Services, CancellationToken.None);
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