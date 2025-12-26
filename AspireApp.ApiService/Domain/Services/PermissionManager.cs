using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Domain service (Manager) for Permission entity.
/// Handles permission-related domain logic and business rules.
/// Throws DomainException for business rule violations.
/// </summary>
public class PermissionManager : DomainService, IPermissionManager
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository;

    public PermissionManager(
        IPermissionRepository permissionRepository,
        IRepository<RolePermission> rolePermissionRepository)
    {
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
    }

    /// <summary>
    /// Creates a new permission with domain validation
    /// </summary>
    public async Task<Permission> CreateAsync(
        string name,
        string description,
        string resource,
        string action,
        CancellationToken cancellationToken = default)
    {
        // Domain validation: Check if permission name already exists
        var existingPermission = await _permissionRepository.GetByNameAsync(name, cancellationToken);
        if (existingPermission != null)
        {
            throw new DomainException(DomainErrors.Permission.NameAlreadyExists(name));
        }

        // Create permission entity
        var permission = new Permission(name, description, resource, action);

        return permission;
    }

    /// <summary>
    /// Validates if permission can be deleted (domain rules)
    /// </summary>
    public async Task ValidateDeletionAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        // Domain validation: Check if permission is being used by any roles
        // Use repository's ExistsAsync method instead of EF Core's AnyAsync
        var isUsedByRoles = await _rolePermissionRepository.ExistsAsync(
            rp => rp.PermissionId == permission.Id && !rp.IsDeleted,
            cancellationToken);

        if (isUsedByRoles)
        {
            throw new DomainException(DomainErrors.Permission.CannotDeleteAssignedPermission(permission.Name));
        }

        // Additional domain validation: Prevent deletion of system-defined permissions
        // You can extend this based on your business rules
        // Example: if (permission.Name.StartsWith("System."))
        // {
        //     throw new DomainException(DomainErrors.Permission.CannotDeleteSystemPermission());
        // }
    }
}

