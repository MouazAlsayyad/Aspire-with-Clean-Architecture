using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Interface for Role domain service (Manager).
/// Handles role-related domain logic and business rules.
/// </summary>
public interface IRoleManager : IDomainService
{
    /// <summary>
    /// Creates a new role with domain validation
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
    /// </summary>
    Task AddPermissionAsync(Role role, Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a permission to a role by permission name
    /// </summary>
    Task AddPermissionAsync(Role role, string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a permission from a role
    /// </summary>
    void RemovePermission(Role role, Guid permissionId);

    /// <summary>
    /// Validates if role can be deleted (domain rules)
    /// </summary>
    void ValidateDeletion(Role role);
}

