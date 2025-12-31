using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Notifications.Domain.Entities;

namespace AspireApp.ApiService.Notifications.Domain.Interfaces;

/// <summary>
/// Interface for Firebase notification manager.
/// Handles sending notifications via Firebase Cloud Messaging.
/// </summary>
public interface IFirebaseNotificationManager : IDomainService
{
    /// <summary>
    /// Sends a notification with action URL and custom data
    /// </summary>
    Task<bool> SendNotificationWithActionAsync(
        Notification notification,
        string fcmToken,
        string language,
        CancellationToken cancellationToken = default);
}

