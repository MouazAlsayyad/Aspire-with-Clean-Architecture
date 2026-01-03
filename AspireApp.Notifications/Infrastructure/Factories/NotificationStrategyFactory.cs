using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Notifications.Infrastructure.Factories;

/// <summary>
/// Factory for creating notification strategies based on channel
/// </summary>
public class NotificationStrategyFactory : INotificationStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationStrategy GetStrategy(NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.All => _serviceProvider.GetRequiredService<IAllNotificationStrategy>(),
            NotificationChannel.TwilioSms => _serviceProvider.GetRequiredService<ITwilioSmsNotificationStrategy>(),
            NotificationChannel.TwilioWhatsApp => _serviceProvider.GetRequiredService<ITwilioWhatsAppNotificationStrategy>(),
            NotificationChannel.Email => _serviceProvider.GetRequiredService<IEmailNotificationStrategy>(),
            NotificationChannel.Firebase => _serviceProvider.GetRequiredService<IFirebaseNotificationStrategy>(),
            _ => throw new ArgumentException($"Unknown notification channel: {channel}", nameof(channel))
        };
    }
}

