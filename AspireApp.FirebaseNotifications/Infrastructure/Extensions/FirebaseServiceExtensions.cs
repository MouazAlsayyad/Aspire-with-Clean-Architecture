using AspireApp.FirebaseNotifications.Infrastructure.RefitClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace AspireApp.FirebaseNotifications.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Firebase services
/// </summary>
public static class FirebaseServiceExtensions
{
    /// <summary>
    /// Registers Firebase services and dependencies
    /// </summary>
    public static IServiceCollection AddFirebaseService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["Firebase:BaseUrl"] ?? "https://fcm.googleapis.com";
        services.AddRefitClient<IFirebaseFcmApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

        return services;
    }
}
