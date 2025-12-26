namespace AspireApp.ApiService.Application.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName
);

