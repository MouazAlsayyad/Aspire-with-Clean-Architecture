using AspireApp.Twilio.Domain.Interfaces;
using AspireApp.Twilio.Domain.Services;
using AspireApp.Twilio.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Twilio.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Twilio services
/// </summary>
public static class TwilioServiceExtensions
{
    /// <summary>
    /// Registers Twilio services and dependencies
    /// </summary>
    public static IServiceCollection AddTwilioService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Twilio client service
        services.AddScoped<ITwilioClientService, TwilioClientService>();
        
        // Register Twilio domain manager
        services.AddScoped<ITwilioSmsManager, TwilioSmsManager>();

        return services;
    }
}

