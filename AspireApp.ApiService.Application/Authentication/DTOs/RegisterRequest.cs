namespace AspireApp.ApiService.Application.Authentication.DTOs;

public record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string? FcmToken = null
);

