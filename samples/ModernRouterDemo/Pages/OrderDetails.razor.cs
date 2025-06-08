using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouterDemo.Pages;

[Route("orders/{Id:int}/details/{LineId:int}/{*Slug}")]
[RouteName("OrderDetails")]
[Breadcrumb("Order Details for Order {Id}", 
    Description = "Detailed information about line item {LineId} for order {Id}", 
    Icon = "📝",
    Order = 2)]
public partial class OrderDetails
{
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public int LineId { get; set; }

    [Parameter]
    public string Slug { get; set; } = string.Empty;
    
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;
}