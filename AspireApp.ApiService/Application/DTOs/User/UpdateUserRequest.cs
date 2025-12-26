namespace AspireApp.ApiService.Application.DTOs.User;

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    bool? IsActive
);

