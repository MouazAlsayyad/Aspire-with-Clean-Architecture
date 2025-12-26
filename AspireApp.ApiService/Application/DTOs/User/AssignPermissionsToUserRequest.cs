namespace AspireApp.ApiService.Application.DTOs.User;

public record AssignPermissionsToUserRequest(
    List<Guid> PermissionIds
);

