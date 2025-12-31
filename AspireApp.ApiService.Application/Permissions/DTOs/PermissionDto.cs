namespace AspireApp.ApiService.Application.Permissions.DTOs;

public record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Resource,
    string Action,
    string FullPermissionName
);

