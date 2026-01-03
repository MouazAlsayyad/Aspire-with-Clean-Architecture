using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.FirebaseNotifications.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(List<Notification> Notifications, bool HasMore)> GetNotificationsAsync(
        Guid userId,
        Guid? lastNotificationId = null,
        int pageSize = 10,
        NotificationTimeFilter timeFilter = NotificationTimeFilter.All,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Notification> query = _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsDeleted);

        // Apply time filter
        var now = DateTime.UtcNow;
        query = timeFilter switch
        {
            NotificationTimeFilter.Today => query.Where(n => n.CreationTime.Date == now.Date),
            NotificationTimeFilter.Yesterday => query.Where(n => n.CreationTime.Date == now.Date.AddDays(-1)),
            NotificationTimeFilter.Earlier => query.Where(n => n.CreationTime.Date < now.Date.AddDays(-1)),
            _ => query
        };

        // Apply ordering
        query = query.OrderByDescending(n => n.CreationTime);

        // Cursor-based pagination
        if (lastNotificationId.HasValue)
        {
            var lastNotification = await _context.Set<Notification>()
                .FirstOrDefaultAsync(n => n.Id == lastNotificationId.Value, cancellationToken);

            if (lastNotification != null)
            {
                query = query.Where(n => n.CreationTime < lastNotification.CreationTime ||
                                        (n.CreationTime == lastNotification.CreationTime && n.Id != lastNotification.Id));
            }
        }

        // Get one extra to check if there are more
        var notifications = await query
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = notifications.Count > pageSize;
        if (hasMore)
        {
            notifications.RemoveAt(notifications.Count - 1);
        }

        return (notifications, hasMore);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted, cancellationToken);
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .OrderByDescending(n => n.CreationTime)
            .ToListAsync(cancellationToken);
    }
}
