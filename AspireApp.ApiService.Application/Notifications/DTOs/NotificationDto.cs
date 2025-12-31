using AspireApp.ApiService.Domain.Notifications.Enums;

namespace AspireApp.ApiService.Application.Notifications.DTOs;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    NotificationPriority Priority,
    NotificationStatus Status,
    bool IsRead,
    DateTime? ReadAt,
    string Title,
    string TitleAr,
    string Message,
    string MessageAr,
    string? ActionUrl,
    Guid UserId,
    DateTime CreationTime
);

