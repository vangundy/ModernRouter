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
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddModernRouter(this IServiceCollection services)
    {
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

        return services;
    }

    /// <summary>
    /// Adds ModernRouter services with authorization support to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureAuthorization">A callback to configure authorization options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddModernRouterWithAuthorization(
        this IServiceCollection services,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        // Register base services
        services.AddModernRouter();

        // Register authorization middleware
        services.AddScoped<INavMiddleware, AuthorizationMiddleware>();

        // Configure authorization options
        var options = new AuthorizationOptions();
        configureAuthorization?.Invoke(options);
        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds ModernRouter services with animation support to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureAnimations">A callback to configure animation options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddModernRouterWithAnimations(
        this IServiceCollection services,
        Action<AnimationOptions>? configureAnimations = null)
    {
        // Register base services
        services.AddModernRouter();

        // Configure animation options
        var options = new AnimationOptions();
        configureAnimations?.Invoke(options);
        services.AddSingleton(options);

        return services;
    }
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