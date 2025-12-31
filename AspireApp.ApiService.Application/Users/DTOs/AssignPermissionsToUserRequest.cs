namespace AspireApp.ApiService.Application.Roles.DTOs;

public record AssignPermissionsToUserRequest(
    List<Guid> PermissionIds
);

