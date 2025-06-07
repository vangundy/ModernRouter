using Microsoft.AspNetCore.Components;
using ModernRouter.Routing;

namespace ModernRouterDemo.Pages;

[Route("details/{LineId:int}/{*Slug}")]
[RouteName("OrderDetails")]
[Breadcrumb("Order Details", 
    Description = "Detailed information about order line items", 
    Icon = "📝",
    Order = 2)]
public partial class OrderDetails
{
    [Parameter]
    public int LineId { get; set; }

    [Parameter]
    public string Slug { get; set; } = string.Empty;
    
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;
}