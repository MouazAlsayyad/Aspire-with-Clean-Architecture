using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Models;
using Microsoft.Extensions.Logging;

namespace AspireApp.Notifications.Infrastructure.Strategies;

/// <summary>
/// Strategy for sending notifications via Firebase Cloud Messaging
/// </summary>
public class FirebaseNotificationStrategy : IFirebaseNotificationStrategy
{
    private readonly IFirebaseFCMService _firebaseFCMService;
    private readonly ILogger<FirebaseNotificationStrategy> _logger;

    public NotificationChannel Channel => NotificationChannel.Firebase;

    public FirebaseNotificationStrategy(
        IFirebaseFCMService firebaseFCMService,
        ILogger<FirebaseNotificationStrategy> logger)
    {
        _firebaseFCMService = firebaseFCMService;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _firebaseFCMService.SendToTokenAsync(
                request.Recipient, // Recipient should be FCM token
                request.Subject,
                request.Body,
                request.Metadata,
                cancellationToken);

            if (success)
            {
                return NotificationResult.Successful(NotificationChannel.Firebase);
            }

            return NotificationResult.Failed(
                NotificationChannel.Firebase,
                "Failed to send Firebase notification");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Firebase notification to token {Token}", request.Recipient);
            return NotificationResult.Failed(NotificationChannel.Firebase, ex.Message);
        }
    }
}

