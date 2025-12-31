using AspireApp.ApiService.Notifications.Domain.Enums;

namespace AspireApp.ApiService.Notifications.Application.DTOs;

public record GetNotificationsRequestDto(
    int PageSize = 20,
    Guid? LastNotificationId = null,
    NotificationTimeFilter TimeFilter = NotificationTimeFilter.All
);

