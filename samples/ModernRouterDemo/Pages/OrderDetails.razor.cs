using Microsoft.AspNetCore.Components;

namespace ModernRouterDemo.Pages;

[Route("details/{LineId:int}/{*Slug}")]
public partial class OrderDetails
{

    [Parameter]
    public int LineId { get; set; }

    [Parameter]
    public string Slug { get; set; } = string.Empty;
}