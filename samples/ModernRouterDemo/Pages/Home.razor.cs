using Microsoft.AspNetCore.Components;

namespace ModernRouterDemo.Pages;
public partial class Home
{
    [Inject]
    public NavigationManager NavManager { get; set; } = default!;
}