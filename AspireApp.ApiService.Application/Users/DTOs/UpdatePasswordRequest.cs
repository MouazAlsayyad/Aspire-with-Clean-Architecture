namespace AspireApp.ApiService.Application.Users.DTOs;

public record UpdatePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

