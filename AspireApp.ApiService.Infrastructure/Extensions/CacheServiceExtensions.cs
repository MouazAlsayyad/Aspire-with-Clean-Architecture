using AspireApp.ApiService.Infrastructure.Services;
using AspireApp.Domain.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.ApiService.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Redis caching services.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Adds Redis caching services to the service collection.
    /// This registers the ICacheService implementation with singleton lifetime.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRedisCaching(this IServiceCollection services)
    {
        // Register ICacheService implementation
        // Using singleton since the IDistributedCache is also typically singleton
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
