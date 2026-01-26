using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AspireApp.FirebaseNotifications.Infrastructure.Repositories;

public class CachedNotificationRepository : CachedRepository<Notification>, INotificationRepository
{
    private readonly INotificationRepository _notificationRepository;

    public CachedNotificationRepository(
        INotificationRepository decorated,
        ICacheService cacheService,
        ILogger<CachedNotificationRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _notificationRepository = decorated;
    }

    public async Task<(List<Notification> Notifications, bool HasMore)> GetNotificationsAsync(
        Guid userId,
        Guid? lastNotificationId = null,
        int pageSize = 10,
        NotificationTimeFilter timeFilter = NotificationTimeFilter.All,
        CancellationToken cancellationToken = default)
    {
        // Pagination logic is hard to cache. Pass through.
        return await _notificationRepository.GetNotificationsAsync(userId, lastNotificationId, pageSize, timeFilter, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string key = $"notification:unread_count:{userId}";
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _notificationRepository.GetUnreadCountAsync(userId, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken);
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string key = $"notification:unread_list:{userId}";
        
        var result = await _cacheService.GetOrSetAsync(
            key,
            ct => _notificationRepository.GetUnreadNotificationsAsync(userId, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken);

        return result ?? new List<Notification>();
    }
}
