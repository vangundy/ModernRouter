using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;
using ModernRouter.Services;
using System.Reflection;

namespace ModernRouter.Components;
public partial class Router
{
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Parameter] public Assembly AppAssembly { get; set; } = Assembly.GetEntryAssembly()!;
    [Parameter] public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }
    [Parameter] public RenderFragment? NotFound { get; set; }
    [Parameter] public RenderFragment<Exception>? ErrorContent { get; set; }

    private List<RouteEntry> _routeTable = [];
    private RouteContext? _current;
    private IReadOnlyList<INavMiddleware> _pipeline = Array.Empty<INavMiddleware>();

    async protected override Task OnInitializedAsync()
    {
        var assemblies = new List<Assembly> { AppAssembly };
        if (AdditionalAssemblies is not null) assemblies.AddRange(AdditionalAssemblies);
        _routeTable = RouteTableFactory.Build(assemblies);

        _pipeline = [.. Services.GetServices<INavMiddleware>()];

        await NavigateAsync(Nav.Uri, firstLoad: true);
        Nav.LocationChanged += async (_, e) =>
            await NavigateAsync(e.Location, firstLoad: false);
    }

    private async Task NavigateAsync(string absoluteUri, bool firstLoad)
    {
        var relative = Nav.ToBaseRelativePath(absoluteUri);
        var match = RouteMatcher.Match(_routeTable, relative);

        var navContext = new NavContext
        {
            TargetUri = absoluteUri,
            Match = match,
            CancellationToken = CancellationToken.None
        };

        var result = await InvokePipelineAsync(navContext, 0);

        if (result.Type == NavResultType.Cancel)
        {
            // undo browser url when cancelled (except initial load)
            if (!firstLoad) Nav.NavigateTo(Nav.Uri, forceLoad: false, replace: true);
            return;
        }
        if (result.RedirectUrl is not null)
        {
            Nav.NavigateTo(result.RedirectUrl, forceLoad: false);
            return;
        }

        _current = match;
        StateHasChanged();
    }

    private Task<NavResult> InvokePipelineAsync(NavContext navContext, int index)
    {
        if (index == _pipeline.Count)
            return Task.FromResult(NavResult.Allow());

        return _pipeline[index].InvokeAsync(navContext, () => InvokePipelineAsync(navContext, index + 1));
    }

}