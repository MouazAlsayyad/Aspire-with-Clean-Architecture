using AspireApp.FirebaseNotifications.Domain.Enums;

namespace AspireApp.FirebaseNotifications.Application.DTOs;

public record CreateNotificationDto(
    NotificationType Type,
    NotificationPriority Priority,
    string Title,
    string TitleAr,
    string Message,
    string MessageAr,
    Guid UserId,
    string? ActionUrl = null
);

