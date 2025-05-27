using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouter.Components;
public partial class RouteView
{
    [Parameter] public RouteEntry Entry { get; set; } = default!;
    [Parameter] public string[] RemainingSegments { get; set; } = [];
    [Parameter] public Dictionary<string, object?> RouteValues { get; set; } = [];

    private IReadOnlyDictionary<string, object?> _parameters
        => RouteValues;
}