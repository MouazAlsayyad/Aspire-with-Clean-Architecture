using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Entities;

namespace AspireApp.ApiService.Domain.Permissions.Services;

/// <summary>
/// Interface for Permission domain service (Manager).
/// Handles permission-related domain logic and business rules.
/// Domain services throw DomainException for business rule violations.
/// </summary>
public interface IPermissionManager : IDomainService
{
    /// <summary>
    /// Creates a new permission with domain validation
    /// Throws DomainException if validation fails
    /// </summary>
    Task<Permission> CreateAsync(
        string name,
        string description,
        string resource,
        string action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if permission can be deleted (domain rules)
    /// Throws DomainException if deletion is not allowed
    /// </summary>
    Task ValidateDeletionAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if permission name already exists
    /// Returns true if permission name exists, false otherwise
    /// </summary>
    Task<bool> PermissionNameExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if permission exists by ID
    /// Returns true if permission exists, false otherwise
    /// </summary>
    Task<bool> PermissionExistsAsync(Guid permissionId, CancellationToken cancellationToken = default);
}

