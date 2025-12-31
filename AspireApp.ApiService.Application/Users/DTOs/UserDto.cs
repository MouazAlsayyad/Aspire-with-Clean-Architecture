namespace AspireApp.ApiService.Application.Users.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    bool IsEmailConfirmed,
    bool IsActive,
    string Language,
    IEnumerable<string> Roles
);

