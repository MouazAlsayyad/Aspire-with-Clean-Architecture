using AspireApp.Notifications.Domain.Enums;

namespace AspireApp.Notifications.Domain.Interfaces;

/// <summary>
/// Factory for creating notification strategies
/// </summary>
public interface INotificationStrategyFactory
{
    /// <summary>
    /// Gets the appropriate strategy for the specified channel
    /// </summary>
    INotificationStrategy GetStrategy(NotificationChannel channel);
}

