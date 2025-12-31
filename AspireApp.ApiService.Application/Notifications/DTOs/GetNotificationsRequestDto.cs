using AspireApp.ApiService.Domain.Notifications.Enums;

namespace AspireApp.ApiService.Application.Notifications.DTOs;

public record GetNotificationsRequestDto(
    int PageSize = 20,
    Guid? LastNotificationId = null,
    NotificationTimeFilter TimeFilter = NotificationTimeFilter.All
);

