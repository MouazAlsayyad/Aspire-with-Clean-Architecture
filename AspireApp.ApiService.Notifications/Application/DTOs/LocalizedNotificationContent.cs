namespace AspireApp.ApiService.Notifications.Application.DTOs;

public record LocalizedNotificationContent(
    string Title,
    string Body,
    string Language,
    string? ActionUrl = null
);

