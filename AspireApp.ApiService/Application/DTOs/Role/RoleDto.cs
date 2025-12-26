namespace AspireApp.ApiService.Application.DTOs.Role;

public record RoleDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    IEnumerable<string> Permissions
);

