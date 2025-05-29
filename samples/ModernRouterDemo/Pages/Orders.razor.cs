using Microsoft.AspNetCore.Components;

namespace ModernRouterDemo.Pages;

[Route("orders/{id:int?}")]
public partial class Orders
{
    [Parameter]
    public int? id { get; set; }
}