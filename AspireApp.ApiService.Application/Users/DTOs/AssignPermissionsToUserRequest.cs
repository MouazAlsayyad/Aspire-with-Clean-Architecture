namespace AspireApp.ApiService.Application.Users.DTOs;

public record AssignPermissionsToUserRequest(
    List<Guid> PermissionIds
);

