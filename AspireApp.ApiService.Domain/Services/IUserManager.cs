using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Interface for User domain service (Manager).
/// Handles user-related domain logic and business rules.
/// Domain services throw DomainException for business rule violations.
/// </summary>
public interface IUserManager : IDomainService
{
    /// <summary>
    /// Creates a new user with domain validation
    /// Throws DomainException if validation fails
    /// </summary>
    Task<User> CreateAsync(
        string email,
        string userName,
        PasswordHash passwordHash,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user
    /// Throws DomainException if role not found
    /// </summary>
    Task AssignRoleAsync(User user, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user by role ID
    /// Throws DomainException if role not found
    /// </summary>
    Task AssignRoleAsync(User user, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    void RemoveRole(User user, Guid roleId);

    /// <summary>
    /// Changes user password
    /// </summary>
    void ChangePassword(User user, PasswordHash newPasswordHash);

    /// <summary>
    /// Activates a user account
    /// </summary>
    void Activate(User user);

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    void Deactivate(User user);

    /// <summary>
    /// Confirms user email
    /// </summary>
    void ConfirmEmail(User user);

    /// <summary>
    /// Validates if user can be deleted (domain rules)
    /// Throws DomainException if deletion is not allowed
    /// </summary>
    void ValidateDeletion(User user);

    /// <summary>
    /// Checks if email already exists
    /// Returns true if email exists, false otherwise
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if username already exists
    /// Returns true if username exists, false otherwise
    /// </summary>
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a permission directly to a user
    /// Throws DomainException if permission not found
    /// </summary>
    Task AssignPermissionAsync(User user, string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a permission directly to a user by permission ID
    /// Throws DomainException if permission not found
    /// </summary>
    Task AssignPermissionAsync(User user, Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a permission directly from a user
    /// </summary>
    void RemovePermission(User user, Guid permissionId);

    /// <summary>
    /// Assigns multiple permissions directly to a user by permission IDs
    /// Throws DomainException if any permission not found
    /// </summary>
    Task AssignPermissionsAsync(User user, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns multiple permissions directly to a user by permission names
    /// Throws DomainException if any permission not found
    /// </summary>
    Task AssignPermissionsAsync(User user, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets (replaces) all permissions directly assigned to a user by permission IDs.
    /// This removes all existing direct permissions and assigns only the new ones.
    /// Throws DomainException if any permission not found
    /// </summary>
    Task SetPermissionsAsync(User user, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets (replaces) all permissions directly assigned to a user by permission names.
    /// This removes all existing direct permissions and assigns only the new ones.
    /// Throws DomainException if any permission not found
    /// </summary>
    Task SetPermissionsAsync(User user, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns multiple roles to a user by role IDs
    /// Throws DomainException if any role not found
    /// </summary>
    Task AssignRolesAsync(User user, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets (replaces) all roles assigned to a user by role IDs.
    /// This removes all existing roles and assigns only the new ones.
    /// Throws DomainException if any role not found
    /// </summary>
    Task SetRolesAsync(User user, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
}

