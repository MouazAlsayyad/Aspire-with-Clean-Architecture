using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Notifications.Domain.Entities;
using AspireApp.ApiService.Notifications.Domain.Enums;

namespace AspireApp.ApiService.Notifications.Domain.Interfaces;

/// <summary>
/// Repository interface for Notification entity
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>
    /// Gets paginated notifications for a user with cursor-based pagination
    /// </summary>
    Task<(List<Notification> Notifications, bool HasMore)> GetNotificationsAsync(
        Guid userId,
        Guid? lastNotificationId = null,
        int pageSize = 10,
        NotificationTimeFilter timeFilter = NotificationTimeFilter.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread notifications count for a user
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unread notifications for a user
    /// </summary>
    Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
}

