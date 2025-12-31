namespace AspireApp.ApiService.Application.Users.DTOs;

public record AssignRoleToUserRequest(
    List<Guid> RoleIds
);

