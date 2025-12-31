using Microsoft.AspNetCore.Authorization;

namespace AspireApp.ApiService.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Check if user has any of the required permissions
        var userPermissions = context.User
            .FindAll("Permission")
            .Select(c => c.Value)
            .ToList();

        var hasPermission = requirement.Permissions
            .Any(permission => userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public IEnumerable<string> Permissions { get; }

    public PermissionRequirement(params string[] permissions)
    {
        Permissions = permissions;
    }
}

