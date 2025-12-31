namespace AspireApp.ApiService.Application.Users.DTOs;

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    bool? IsActive
);

