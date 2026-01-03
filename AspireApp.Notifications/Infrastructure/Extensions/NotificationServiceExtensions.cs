using AspireApp.Notifications.Application.Validators;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Services;
using AspireApp.Notifications.Infrastructure.Factories;
using AspireApp.Notifications.Infrastructure.Strategies;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Notifications.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering notification services
/// </summary>
public static class NotificationServiceExtensions
{
    /// <summary>
    /// Registers all notification strategies and related services
    /// </summary>
    public static IServiceCollection AddNotificationStrategies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register all strategies
        services.AddScoped<ITwilioSmsNotificationStrategy, TwilioSmsNotificationStrategy>();
        services.AddScoped<ITwilioWhatsAppNotificationStrategy, TwilioWhatsAppNotificationStrategy>();
        services.AddScoped<IEmailNotificationStrategy, EmailNotificationStrategy>();
        services.AddScoped<IFirebaseNotificationStrategy, FirebaseNotificationStrategy>();
        services.AddScoped<IAllNotificationStrategy, AllNotificationStrategy>();

        // Register factory
        services.AddScoped<INotificationStrategyFactory, NotificationStrategyFactory>();

        // Register orchestrator
        services.AddScoped<NotificationOrchestrator>();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<SendNotificationDtoValidator>();

        return services;
    }
}

