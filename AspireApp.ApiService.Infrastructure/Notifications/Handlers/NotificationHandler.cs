using AspireApp.ApiService.Domain.Notifications.Events;
using AspireApp.ApiService.Domain.Notifications.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Infrastructure.DomainEvents;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Notifications.Handlers;

/// <summary>
/// Handles NotificationCreatedEvent and sends notification via Firebase
/// </summary>
public class NotificationHandler : IDomainEventHandler<NotificationCreatedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFirebaseNotificationManager _firebaseNotificationManager;
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IFirebaseNotificationManager firebaseNotificationManager,
        ILogger<NotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _firebaseNotificationManager = firebaseNotificationManager;
        _logger = logger;
    }

    public async Task HandleAsync(NotificationCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get notification
            var notification = await _notificationRepository.GetAsync(domainEvent.NotificationId, cancellationToken: cancellationToken);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found when handling NotificationCreatedEvent",
                    domainEvent.NotificationId);
                return;
            }

            // Get user to retrieve FCM token and language
            var user = await _userRepository.GetAsync(domainEvent.UserId, cancellationToken: cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when handling NotificationCreatedEvent",
                    domainEvent.UserId);
                notification.UpdateStatus(Domain.Notifications.Enums.NotificationStatus.Failed);
                await _notificationRepository.UpdateAsync(notification, cancellationToken);
                return;
            }

            // Check if user has FCM token
            if (string.IsNullOrWhiteSpace(user.FcmToken))
            {
                _logger.LogInformation("User {UserId} does not have FCM token. Notification {NotificationId} will not be sent via push.",
                    domainEvent.UserId, domainEvent.NotificationId);
                // Don't mark as failed - notification is still stored in database
                return;
            }

            // Get user language preference
            var language = user.Language ?? "en";

            // Send notification via Firebase
            var success = await _firebaseNotificationManager.SendNotificationWithActionAsync(
                notification,
                user.FcmToken,
                language,
                cancellationToken);

            // Update notification status
            notification.UpdateStatus(success ? Domain.Notifications.Enums.NotificationStatus.Sent : Domain.Notifications.Enums.NotificationStatus.Failed);
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling NotificationCreatedEvent for notification {NotificationId}",
                domainEvent.NotificationId);

            // Try to update notification status to failed
            try
            {
                var notification = await _notificationRepository.GetAsync(domainEvent.NotificationId, cancellationToken: cancellationToken);
                if (notification != null)
                {
                    notification.UpdateStatus(Domain.Notifications.Enums.NotificationStatus.Failed);
                    await _notificationRepository.UpdateAsync(notification, cancellationToken);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update notification status after error");
            }
        }
    }
}

