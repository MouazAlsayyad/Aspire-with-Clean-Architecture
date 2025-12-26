using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace AspireApp.ApiService.Presentation.Extensions;

/// <summary>
/// Extension methods for RouteHandlerBuilder to add permission and role-based authorization.
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Requires the user to have at least one of the specified permissions.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="permissions">The permission names to require.</param>
    /// <returns>The route handler builder for method chaining.</returns>
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified.", nameof(permissions));
        }

        // Use the PermissionPolicyProvider format: "Permission:permission1,permission2"
        var policyName = $"Permission:{string.Join(",", permissions)}";
        return builder.RequireAuthorization(policyName);
    }

    /// <summary>
    /// Requires the user to have at least one of the specified roles.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="roles">The role names to require.</param>
    /// <returns>The route handler builder for method chaining.</returns>
    public static RouteHandlerBuilder RequireRole(
        this RouteHandlerBuilder builder,
        params string[] roles)
    {
        if (roles == null || roles.Length == 0)
        {
            throw new ArgumentException("At least one role must be specified.", nameof(roles));
        }

        // Use ASP.NET Core's built-in role authorization
        return builder.RequireAuthorization(policy => policy.RequireRole(roles));
    }
}

