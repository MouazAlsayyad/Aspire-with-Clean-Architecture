namespace AspireApp.ApiService.Application.DTOs.Permission;

public record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Resource,
    string Action,
    string FullPermissionName
);

