namespace AspireApp.ApiService.Application.Roles.DTOs;

public record UpdateRoleRequest(
    string? Description,
    IEnumerable<Guid>? PermissionIds = null
);

