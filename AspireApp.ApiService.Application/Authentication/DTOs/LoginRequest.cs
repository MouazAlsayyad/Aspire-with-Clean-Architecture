namespace AspireApp.ApiService.Application.Authentication.DTOs;

public record LoginRequest(
    string Email,
    string Password
);

