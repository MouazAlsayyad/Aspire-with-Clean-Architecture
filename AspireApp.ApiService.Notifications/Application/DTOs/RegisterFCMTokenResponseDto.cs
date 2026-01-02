namespace AspireApp.ApiService.Notifications.Application.DTOs;

public record RegisterFCMTokenResponseDto(
    bool Success,
    string Message
);

