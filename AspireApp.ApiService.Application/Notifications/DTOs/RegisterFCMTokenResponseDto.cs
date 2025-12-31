namespace AspireApp.ApiService.Application.Notifications.DTOs;

public record RegisterFCMTokenResponseDto(
    bool Success,
    string Message
);

