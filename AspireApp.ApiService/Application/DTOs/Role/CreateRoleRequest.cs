using AspireApp.ApiService.Domain.Enums;

namespace AspireApp.ApiService.Application.DTOs.Role;

public record CreateRoleRequest(
    string Name,
    string Description,
    RoleType Type,
    IEnumerable<Guid>? PermissionIds = null
);

