using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;
using ModernRouter.Services;
using System.Reflection;

namespace ModernRouter.Components;
public partial class Router
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Parameter] public Assembly AppAssembly { get; set; } = Assembly.GetEntryAssembly()!;
    [Parameter] public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }
    [Parameter] public RenderFragment? NotFound { get; set; }

    private List<RouteEntry> _routeTable = [];
    private RouteContext? _current;

    protected override void OnInitialized()
    {
        var assemblies = new List<Assembly> { AppAssembly };
        if (AdditionalAssemblies is not null) assemblies.AddRange(AdditionalAssemblies);
        _routeTable = RouteTableFactory.Build(assemblies);

        NavigateTo(Nav.Uri);
        Nav.LocationChanged += (_, e) => NavigateTo(e.Location);
    }

    private void NavigateTo(string absoluteUri)
    {
        var relative = Nav.ToBaseRelativePath(absoluteUri);
        _current = RouteMatcher.Match(_routeTable, relative);
        StateHasChanged();
    }
}