using AspireApp.ApiService.Domain.Notifications.Entities;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Domain.Notifications.Interfaces;

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

