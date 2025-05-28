using Microsoft.Extensions.DependencyInjection;
using ModernRouter.Routing;
using ModernRouter.Security;

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
    public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            return NavResult.Error(ex);
        }
    }
}