namespace AspireApp.ApiService.Application.DTOs.User;

public record UpdatePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

