using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Animations;
using ModernRouter.Routing;
using ModernRouter.Security;
using ModernRouter.Services;

namespace ModernRouter.Extensions;

/// <summary>
/// Extension methods for setting up ModernRouter services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ModernRouter services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureOptions">A callback to configure ModernRouter options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddModernRouter(
        this IServiceCollection services,
        Action<ModernRouterOptions>? configureOptions = null)
    {
        // Configure options
        var options = new ModernRouterOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register core services needed by ModernRouter
        services.AddScoped<INavMiddleware, ErrorHandlingMiddleware>();
        services.AddSingleton<IRouteTableService, RouteTableService>();
        services.AddSingleton<IRouteNameService, RouteNameService>();
        services.AddSingleton<IBreadcrumbService, BreadcrumbService>();

        // Register JSRuntime wrapper for testability
        services.AddSingleton<IJSRuntimeWrapper, JSRuntimeWrapper>();
        services.AddSingleton<IRouteAnimationService, RouteAnimationService>();

        // Register NavigationManager wrapper for testability
        services.AddScoped<INavigationWrapper, NavigationWrapper>();

        // Register new consolidated services
        services.AddScoped<IRouteMatchingService, RouteMatchingService>();
        services.AddScoped<IRouteRenderingService, RouteRenderingService>();

        // Conditionally register authorization middleware
        if (options.EnableAuthorization)
        {
            services.AddScoped<INavMiddleware, AuthorizationMiddleware>();
            services.AddSingleton(options.Authorization);
        }

        // Animation options are always registered since animations can be enabled/disabled
        services.AddSingleton(options.Animations);

        return services;
    }

}

/// <summary>
/// Comprehensive options for configuring ModernRouter.
/// </summary>
public class ModernRouterOptions
{
    /// <summary>
    /// Gets or sets whether authorization middleware should be enabled.
    /// </summary>
    public bool EnableAuthorization { get; set; } = false;

    /// <summary>
    /// Gets or sets the authorization options.
    /// </summary>
    public AuthorizationOptions Authorization { get; set; } = new();

    /// <summary>
    /// Gets or sets the animation options.
    /// </summary>
    public AnimationOptions Animations { get; set; } = new();
}

/// <summary>
/// Options for configuring ModernRouter animations.
/// </summary>
public class AnimationOptions
{
    /// <summary>
    /// Gets or sets whether animations are enabled globally.
    /// </summary>
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// Gets or sets the default animation duration in milliseconds.
    /// </summary>
    public int DefaultDuration { get; set; } = 300;

    /// <summary>
    /// Gets or sets the default animation easing.
    /// </summary>
    public AnimationEasing DefaultEasing { get; set; } = AnimationEasing.EaseInOut;

    /// <summary>
    /// Gets or sets whether to respect the user's reduced motion preference.
    /// </summary>
    public bool RespectReducedMotion { get; set; } = true;
}

/// <summary>
/// Options for configuring ModernRouter authorization.
/// </summary>
public class AuthorizationOptions
{
    /// <summary>
    /// Gets or sets the path to redirect to when authentication is required.
    /// </summary>
    public string LoginPath { get; set; } = "/login";

    /// <summary>
    /// Gets or sets the path to redirect to when authorization fails.
    /// </summary>
    public string ForbiddenPath { get; set; } = "/forbidden";

    /// <summary>
    /// Gets or sets whether to include the return URL when redirecting to the login page.
    /// </summary>
    public bool IncludeReturnUrl { get; set; } = true;
}

/// <summary>
/// Default middleware that handles errors during navigation.
/// </summary>
internal class ErrorHandlingMiddleware : INavMiddleware
{
    public async Task<NavResult> InvokeAsync(NavContext context, Func<Task<NavResult>> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            // Log the exception if a logger is available
            // Note: Logging should be configured by the application, not the router library
            return NavResult.Error(ex);
        }
    }
}