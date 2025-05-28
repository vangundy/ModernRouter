using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ModernRouter.Routing;
using ModernRouterDemo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Services.AddScoped<INavMiddleware, AnalyticsTap>();
//builder.Services.AddScoped<INavMiddleware, AuthGuard>();
//builder.Services.AddScoped<INavMiddleware, UnsavedGuard>();

await builder.Build().RunAsync();
