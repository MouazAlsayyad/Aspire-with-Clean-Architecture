namespace AspireApp.FirebaseNotifications.Application.DTOs;

public record GetNotificationsResponseDto(
    List<NotificationDto> Notifications,
    bool HasMore,
    Guid? LastNotificationId
);

