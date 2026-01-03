using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;

namespace AspireApp.FirebaseNotifications.Domain.Interfaces;

/// <summary>
/// Interface for Notification domain service (Manager).
/// Handles notification-related domain logic and business rules.
/// </summary>
public interface INotificationManager : IDomainService
{
    /// <summary>
    /// Creates a new notification
    /// </summary>
    Task<Notification> CreateNotificationAsync(
        NotificationType type,
        NotificationPriority priority,
        string title,
        string titleAr,
        string message,
        string messageAr,
        Guid userId,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a notification via Firebase
    /// </summary>
    Task PublishNotificationAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated notifications for a user
    /// </summary>
    Task<(List<Notification> Notifications, bool HasMore)> GetNotificationsAsync(
        Guid userId,
        Guid? lastNotificationId = null,
        int pageSize = 10,
        NotificationTimeFilter timeFilter = NotificationTimeFilter.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates notification read status
    /// </summary>
    Task UpdateNotificationStatusAsync(Guid notificationId, bool isRead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications as read for a user
    /// </summary>
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

