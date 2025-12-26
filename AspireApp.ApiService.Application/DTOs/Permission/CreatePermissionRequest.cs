namespace AspireApp.ApiService.Application.DTOs.Permission;

public record CreatePermissionRequest(
    string Name,
    string Description,
    string Resource,
    string Action
);

