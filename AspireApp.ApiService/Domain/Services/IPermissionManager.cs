using AspireApp.ApiService.Domain.Entities;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Interface for Permission domain service (Manager).
/// Handles permission-related domain logic and business rules.
/// </summary>
public interface IPermissionManager : IDomainService
{
    /// <summary>
    /// Creates a new permission with domain validation
    /// </summary>
    Task<Permission> CreateAsync(
        string name,
        string description,
        string resource,
        string action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if permission can be deleted (domain rules)
    /// </summary>
    Task ValidateDeletionAsync(Permission permission, CancellationToken cancellationToken = default);
}

