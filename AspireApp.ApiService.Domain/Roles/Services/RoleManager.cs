using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Services;

namespace AspireApp.ApiService.Domain.Roles.Services;

/// <summary>
/// Domain service (Manager) for Role entity.
/// Handles role-related domain logic and business rules.
/// Throws DomainException for business rule violations.
/// </summary>
public class RoleManager : DomainService, IRoleManager
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleManager(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    /// <summary>
    /// Creates a new role.
    /// Note: Role name uniqueness validation is handled by FluentValidation at the application layer.
    /// </summary>
    public async Task<Role> CreateAsync(
        string name,
        string description,
        RoleType type,
        CancellationToken cancellationToken = default)
    {
        // Create role entity
        // Validation for role name uniqueness is handled by FluentValidation validators
        var role = new Role(name, description, type);

        return role;
    }

    /// <summary>
    /// Updates role description
    /// </summary>
    public void UpdateDescription(Role role, string description)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        role.UpdateDescription(description);
    }

    /// <summary>
    /// Adds a permission to a role
    /// </summary>
    public async Task AddPermissionAsync(Role role, Guid permissionId, CancellationToken cancellationToken = default)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var permission = await _permissionRepository.GetAsync(permissionId, cancellationToken: cancellationToken);
        if (permission == null)
        {
            throw new DomainException(DomainErrors.Permission.NotFound(permissionId));
        }

        role.AddPermission(permission);
    }

    /// <summary>
    /// Adds a permission to a role by permission name
    /// </summary>
    public async Task AddPermissionAsync(Role role, string permissionName, CancellationToken cancellationToken = default)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var permission = await _permissionRepository.GetByNameAsync(permissionName, cancellationToken);
        if (permission == null)
        {
            throw new DomainException(DomainErrors.Permission.NotFound(permissionName));
        }

        role.AddPermission(permission);
    }

    /// <summary>
    /// Removes a permission from a role
    /// </summary>
    public void RemovePermission(Role role, Guid permissionId)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        role.RemovePermission(permissionId);
    }

    /// <summary>
    /// Validates if role can be deleted (domain rules)
    /// </summary>
    public void ValidateDeletion(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        // Add domain-specific validation rules here
        // For example: Cannot delete admin roles, etc.
        if (role.Type == RoleType.Admin)
        {
            // Optionally prevent deletion of admin roles
            // throw new DomainException(DomainErrors.Role.CannotDeleteAdminRole());
        }
    }

    /// <summary>
    /// Checks if role name already exists
    /// </summary>
    public async Task<bool> RoleNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var existingRole = await _roleRepository.GetByNameAsync(name, cancellationToken);
        return existingRole != null;
    }

    /// <summary>
    /// Checks if permission exists by ID
    /// </summary>
    public async Task<bool> PermissionExistsAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetAsync(permissionId, cancellationToken: cancellationToken);
        return permission != null;
    }

    /// <summary>
    /// Checks if role exists by ID
    /// </summary>
    public async Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetAsync(roleId, cancellationToken: cancellationToken);
        return role != null;
    }
}

