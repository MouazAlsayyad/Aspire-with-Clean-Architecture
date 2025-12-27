namespace AspireApp.ApiService.Application.DTOs.User;

public record CreateUserRequest(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    bool? IsActive = null
);

