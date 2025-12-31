namespace AspireApp.ApiService.Application.Roles.DTOs;

public record RoleDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    IEnumerable<string> Permissions
);

