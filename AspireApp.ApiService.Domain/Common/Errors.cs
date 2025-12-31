namespace AspireApp.ApiService.Domain.Common;

/// <summary>
/// Predefined domain errors following DDD principles.
/// These are domain-specific errors that represent business rule violations.
/// </summary>
public static class DomainErrors
{
    public static class User
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("User.NotFound", $"User with ID '{id}' was not found.");

        public static Error NotFound(string email) =>
            Error.NotFound("User.NotFound", $"User with email '{email}' was not found.");

        public static Error EmailAlreadyExists(string email) =>
            Error.Conflict("User.EmailAlreadyExists", $"User with email '{email}' already exists.");

        public static Error UserNameAlreadyExists(string userName) =>
            Error.Conflict("User.UserNameAlreadyExists", $"User with username '{userName}' already exists.");

        public static Error InvalidCredentials() =>
            Error.Unauthorized("User.InvalidCredentials", "Invalid email or password.");

        public static Error AccountDeactivated() =>
            Error.Forbidden("User.AccountDeactivated", "User account is deactivated.");

        public static Error EmailNotConfirmed() =>
            Error.Forbidden("User.EmailNotConfirmed", "User email is not confirmed.");

        public static Error InvalidPassword() =>
            Error.Validation("User.InvalidPassword", "Password does not meet requirements.");
    }

    public static class Role
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Role.NotFound", $"Role with ID '{id}' was not found.");

        public static Error NotFound(string name) =>
            Error.NotFound("Role.NotFound", $"Role with name '{name}' was not found.");

        public static Error NameAlreadyExists(string name) =>
            Error.Conflict("Role.NameAlreadyExists", $"Role with name '{name}' already exists.");

        public static Error CannotDeleteAssignedRole(string roleName) =>
            Error.Conflict("Role.CannotDeleteAssignedRole", $"Cannot delete role '{roleName}' because it is assigned to users.");
    }

    public static class Permission
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Permission.NotFound", $"Permission with ID '{id}' was not found.");

        public static Error NotFound(string name) =>
            Error.NotFound("Permission.NotFound", $"Permission with name '{name}' was not found.");

        public static Error NameAlreadyExists(string name) =>
            Error.Conflict("Permission.NameAlreadyExists", $"Permission with name '{name}' already exists.");

        public static Error CannotDeleteAssignedPermission(string permissionName) =>
            Error.Conflict("Permission.CannotDeleteAssignedPermission",
                $"Permission '{permissionName}' cannot be deleted because it is assigned to one or more roles. Please remove the permission from all roles before deleting.");
    }

    public static class RefreshToken
    {
        public static Error Invalid() =>
            Error.Unauthorized("RefreshToken.Invalid", "Invalid or expired refresh token.");

        public static Error NotFound() =>
            Error.NotFound("RefreshToken.NotFound", "Refresh token not found.");

        public static Error Revoked() =>
            Error.Unauthorized("RefreshToken.Revoked", "Refresh token has been revoked.");

        public static Error Expired() =>
            Error.Unauthorized("RefreshToken.Expired", "Refresh token has expired.");

        public static Error Reused() =>
            Error.Unauthorized("RefreshToken.Reused", "Refresh token has been reused. All tokens for this account have been revoked for security reasons.");
    }

    public static class General
    {
        public static Error Unauthorized() =>
            Error.Unauthorized("General.Unauthorized", "You are not authorized to perform this action.");

        public static Error Forbidden() =>
            Error.Forbidden("General.Forbidden", "You do not have permission to perform this action.");

        public static Error ServerError(string message) =>
            Error.Failure("General.ServerError", $"An error occurred: {message}");

        public static Error NotFound(string message) =>
            Error.NotFound("General.NotFound", message);

        public static Error InvalidInput(string message) =>
            Error.Validation("General.InvalidInput", message);

        public static Error InternalError(string message) =>
            Error.Failure("General.InternalError", message);
    }
}

