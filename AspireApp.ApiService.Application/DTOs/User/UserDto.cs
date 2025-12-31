namespace AspireApp.ApiService.Application.DTOs.User;

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

