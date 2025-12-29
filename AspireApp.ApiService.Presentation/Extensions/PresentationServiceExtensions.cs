using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.ApiService.Presentation.Extensions;

/// <summary>
/// Extension methods for configuring Presentation layer services
/// </summary>
public static class PresentationServiceExtensions
{
    /// <summary>
    /// Configures OpenAPI/Swagger services
    /// </summary>
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        return services;
    }
}

