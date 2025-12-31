namespace AspireApp.ApiService.Application.Notifications.DTOs;

public record GetNotificationsResponseDto(
    List<NotificationDto> Notifications,
    bool HasMore,
    Guid? LastNotificationId
);

