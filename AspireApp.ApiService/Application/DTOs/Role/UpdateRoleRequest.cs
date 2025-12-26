namespace AspireApp.ApiService.Application.DTOs.Role;

public record UpdateRoleRequest(
    string? Description,
    IEnumerable<Guid>? PermissionIds = null
);

