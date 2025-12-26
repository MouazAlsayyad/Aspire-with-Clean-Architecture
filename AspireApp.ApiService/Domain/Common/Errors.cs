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
    }

    public static class General
    {
        public static Error Unauthorized() => 
            Error.Unauthorized("General.Unauthorized", "You are not authorized to perform this action.");

        public static Error Forbidden() => 
            Error.Forbidden("General.Forbidden", "You do not have permission to perform this action.");

        public static Error ServerError(string message) => 
            Error.Failure("General.ServerError", $"An error occurred: {message}");
    }
}

