using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Domain.Roles.Services;

/// <summary>
/// Interface for Role domain service (Manager).
/// Handles role-related domain logic and business rules.
/// Domain services throw DomainException for business rule violations.
/// </summary>
public interface IRoleManager : IDomainService
{
    /// <summary>
    /// Creates a new role with domain validation
    /// Throws DomainException if validation fails
    /// </summary>
    Task<Role> CreateAsync(
        string name,
        string description,
        RoleType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates role description
    /// </summary>
    void UpdateDescription(Role role, string description);

    /// <summary>
    /// Adds a permission to a role
    /// Throws DomainException if permission not found
    /// </summary>
    Task AddPermissionAsync(Role role, Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a permission to a role by permission name
    /// Throws DomainException if permission not found
    /// </summary>
    Task AddPermissionAsync(Role role, string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a permission from a role
    /// </summary>
    void RemovePermission(Role role, Guid permissionId);

    /// <summary>
    /// Validates if role can be deleted (domain rules)
    /// Throws DomainException if deletion is not allowed
    /// </summary>
    void ValidateDeletion(Role role);

    /// <summary>
    /// Checks if role name already exists
    /// Returns true if role name exists, false otherwise
    /// </summary>
    Task<bool> RoleNameExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if permission exists by ID
    /// Returns true if permission exists, false otherwise
    /// </summary>
    Task<bool> PermissionExistsAsync(Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if role exists by ID
    /// Returns true if role exists, false otherwise
    /// </summary>
    Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken = default);
}

