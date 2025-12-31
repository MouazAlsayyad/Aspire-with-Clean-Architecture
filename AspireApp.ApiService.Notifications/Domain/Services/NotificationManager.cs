using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Notifications.Domain.Entities;
using AspireApp.ApiService.Notifications.Domain.Enums;
using AspireApp.ApiService.Notifications.Domain.Events;
using AspireApp.ApiService.Notifications.Domain.Interfaces;

namespace AspireApp.ApiService.Notifications.Domain.Services;

/// <summary>
/// Domain service (Manager) for Notification entity.
/// Handles notification-related domain logic and business rules.
/// </summary>
public class NotificationManager : DomainService, INotificationManager
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public NotificationManager(
        INotificationRepository notificationRepository,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _notificationRepository = notificationRepository;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<Notification> CreateNotificationAsync(
        NotificationType type,
        NotificationPriority priority,
        string title,
        string titleAr,
        string message,
        string messageAr,
        Guid userId,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification(
            type,
            priority,
            title,
            titleAr,
            message,
            messageAr,
            userId,
            actionUrl);

        await _notificationRepository.AddAsync(notification, cancellationToken);

        // Raise domain event
        notification.AddDomainEvent(new NotificationCreatedEvent(notification.Id, userId));

        return notification;
    }

    public async Task PublishNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // This method will be called by the domain event handler
        // The actual Firebase sending is handled by FirebaseNotificationManager
        // Status will be updated by the handler
    }

    public async Task<(List<Notification> Notifications, bool HasMore)> GetNotificationsAsync(
        Guid userId,
        Guid? lastNotificationId = null,
        int pageSize = 10,
        NotificationTimeFilter timeFilter = NotificationTimeFilter.All,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetNotificationsAsync(
            userId,
            lastNotificationId,
            pageSize,
            timeFilter,
            cancellationToken);
    }

    public async Task UpdateNotificationStatusAsync(Guid notificationId, bool isRead, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetAsync(notificationId, cancellationToken: cancellationToken)
            ?? throw new DomainException(new Error("Notification.NotFound", $"Notification with ID {notificationId} not found"));

        if (isRead)
            notification.MarkAsRead();
        else
            notification.MarkAsUnread();

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
        
        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        if (unreadNotifications.Any())
        {
            await _notificationRepository.UpdateRangeAsync(unreadNotifications, cancellationToken);
        }

        return unreadNotifications.Count;
    }
}

