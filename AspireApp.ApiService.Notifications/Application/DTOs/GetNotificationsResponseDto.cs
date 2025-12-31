namespace AspireApp.ApiService.Notifications.Application.DTOs;

public record GetNotificationsResponseDto(
    List<NotificationDto> Notifications,
    bool HasMore,
    Guid? LastNotificationId
);

