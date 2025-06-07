using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouterDemo.Pages;

[Route("orders/{Id:int?}")]
[RouteName("Orders")]
[Breadcrumb("Orders", 
    Description = "View and manage customer orders", 
    Icon = "📋",
    Order = 1)]
public partial class Orders
{
    [Parameter]
    public int? Id { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;
}