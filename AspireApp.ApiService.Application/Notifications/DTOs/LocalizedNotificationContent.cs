namespace AspireApp.ApiService.Application.Notifications.DTOs;

public record LocalizedNotificationContent(
    string Title,
    string Body,
    string Language,
    string? ActionUrl = null
);

