using AspireApp.ApiService.Application.DTOs.User;

namespace AspireApp.ApiService.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

