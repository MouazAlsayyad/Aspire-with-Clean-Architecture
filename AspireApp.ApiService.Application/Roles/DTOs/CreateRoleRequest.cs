using AspireApp.ApiService.Domain.Roles.Enums;

namespace AspireApp.ApiService.Application.Roles.DTOs;

public record CreateRoleRequest(
    string Name,
    string Description,
    RoleType Type,
    IEnumerable<Guid>? PermissionIds = null
);

