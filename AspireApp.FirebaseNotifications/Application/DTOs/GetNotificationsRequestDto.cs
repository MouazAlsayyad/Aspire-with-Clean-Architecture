using AspireApp.FirebaseNotifications.Domain.Enums;

namespace AspireApp.FirebaseNotifications.Application.DTOs;

public record GetNotificationsRequestDto(
    int PageSize = 20,
    Guid? LastNotificationId = null,
    NotificationTimeFilter TimeFilter = NotificationTimeFilter.All
);

