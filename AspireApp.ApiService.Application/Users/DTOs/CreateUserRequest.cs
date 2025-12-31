namespace AspireApp.ApiService.Application.Users.DTOs;

public record CreateUserRequest(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    bool? IsActive = null
);

