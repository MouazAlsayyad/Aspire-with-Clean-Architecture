using AspireApp.ApiService.Application.Users.DTOs;

namespace AspireApp.ApiService.Application.Authentication.DTOs;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

