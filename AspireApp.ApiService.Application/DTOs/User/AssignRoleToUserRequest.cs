namespace AspireApp.ApiService.Application.DTOs.User;

public record AssignRoleToUserRequest(
    List<Guid> RoleIds
);

