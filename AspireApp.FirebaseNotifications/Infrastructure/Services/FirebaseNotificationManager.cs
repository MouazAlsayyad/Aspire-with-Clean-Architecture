using AspireApp.ApiService.Domain.Services;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.FirebaseNotifications.Infrastructure.Services;

/// <summary>
/// Firebase notification manager implementation.
/// Orchestrates sending notifications via Firebase Cloud Messaging.
/// </summary>
public class FirebaseNotificationManager : DomainService, IFirebaseNotificationManager
{
    private readonly IFirebaseFCMService _fcmService;
    private readonly ILogger<FirebaseNotificationManager> _logger;

    public FirebaseNotificationManager(
        IFirebaseFCMService fcmService,
        ILogger<FirebaseNotificationManager> logger)
    {
        _fcmService = fcmService;
        _logger = logger;
    }

    public async Task<bool> SendNotificationWithActionAsync(
        Notification notification,
        string fcmToken,
        string language,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (string.IsNullOrWhiteSpace(fcmToken))
        {
            _logger.LogWarning("FCM token is empty for notification {NotificationId}", notification.Id);
            return false;
        }

        // Select title and message based on language
        var title = language == "ar" ? notification.TitleAr : notification.Title;
        var body = language == "ar" ? notification.MessageAr : notification.Message;

        // Prepare custom data
        var data = new Dictionary<string, string>
        {
            { "notificationId", notification.Id.ToString() },
            { "type", ((int)notification.Type).ToString() },
            { "priority", ((int)notification.Priority).ToString() }
        };

        if (!string.IsNullOrWhiteSpace(notification.ActionUrl))
        {
            data["actionUrl"] = notification.ActionUrl;
        }

        // Send notification
        var success = await _fcmService.SendToTokenAsync(
            fcmToken,
            title,
            body,
            data,
            cancellationToken);

        if (success)
        {
            _logger.LogInformation("Successfully sent notification {NotificationId} to user {UserId}", 
                notification.Id, notification.UserId);
        }
        else
        {
            _logger.LogWarning("Failed to send notification {NotificationId} to user {UserId}", 
                notification.Id, notification.UserId);
        }

        return success;
    }
}

