using Microsoft.Extensions.DependencyInjection;

namespace ModernRouter.Extensions;

/// <summary>
/// Helper class for accessing services from static contexts
/// </summary>
internal static class ServiceProviderHelper
{
    private static IServiceProvider? _serviceProvider;
    
    /// <summary>
    /// Initializes the service provider helper
    /// </summary>
    /// <param name="serviceProvider">Service provider instance</param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Gets a required service of the specified type
    /// </summary>
    /// <typeparam name="T">Type of service to get</typeparam>
    /// <returns>Service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service provider is not initialized</exception>
    public static T GetRequiredService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("ServiceProviderHelper not initialized");
            
        return _serviceProvider.GetRequiredService<T>();
    }
}