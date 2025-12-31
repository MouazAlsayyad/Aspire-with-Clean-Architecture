namespace AspireApp.ApiService.Application.Permissions.DTOs;

public record CreatePermissionRequest(
    string Name,
    string Description,
    string Resource,
    string Action
);

