using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Components;
public partial class Outlet
{
    [CascadingParameter] private List<RouteEntry>? RouteTable { get; set; }
    [CascadingParameter] private string[]? Segments { get; set; }

    [CascadingParameter] private RouteContext RouteContext { get; set; } = default!;

    protected override void OnParametersSet()
    {
        if (RouteTable is null || Segments is null) return;
        RouteContext = RouteMatcher.Match(RouteTable, string.Join('/', Segments));
    }
}