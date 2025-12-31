using AspireApp.ApiService.Domain.Notifications.Enums;

namespace AspireApp.ApiService.Application.Notifications.DTOs;

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

