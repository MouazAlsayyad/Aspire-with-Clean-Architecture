using AspireApp.ApiService.Notifications.Domain.Enums;

namespace AspireApp.ApiService.Notifications.Application.DTOs;

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

