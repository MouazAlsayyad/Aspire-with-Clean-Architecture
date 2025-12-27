using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; } = false;
    public bool IsActive { get; private set; } = true;

    // Navigation property for many-to-many relationship with roles
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Navigation property for many-to-many relationship with permissions (direct assignment)
    private readonly List<UserPermission> _userPermissions = [];
    public IReadOnlyCollection<UserPermission> UserPermissions => _userPermissions.AsReadOnly();

    // Private constructor for EF Core
    private User() { }

    public User(string email, string userName, PasswordHash passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName cannot be empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty", nameof(lastName));

        Email = email.ToLowerInvariant();
        UserName = userName;
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdatePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        SetLastModificationTime();
    }

    public void UpdateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
        
        FirstName = firstName;
        SetLastModificationTime();
    }

    public void UpdateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty", nameof(lastName));
        
        LastName = lastName;
        SetLastModificationTime();
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        SetLastModificationTime();
    }

    public void Activate()
    {
        IsActive = true;
        SetLastModificationTime();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetLastModificationTime();
    }

    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return; // Role already assigned

        var userRole = new UserRole(Id, role.Id);
        _userRoles.Add(userRole);
        SetLastModificationTime();
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            SetLastModificationTime();
        }
    }

    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (_userPermissions.Any(up => up.PermissionId == permission.Id))
            return; // Permission already assigned

        var userPermission = new UserPermission(Id, permission.Id);
        _userPermissions.Add(userPermission);
        SetLastModificationTime();
    }

    public void RemovePermission(Guid permissionId)
    {
        var userPermission = _userPermissions.FirstOrDefault(up => up.PermissionId == permissionId);
        if (userPermission != null)
        {
            _userPermissions.Remove(userPermission);
            SetLastModificationTime();
        }
    }

    /// <summary>
    /// Sets (replaces) all permissions for the user with the provided permissions.
    /// Uses a diff-based approach to minimize database operations and avoid concurrency issues.
    /// </summary>
    public void SetPermissions(IEnumerable<Permission> permissions)
    {
        if (permissions == null)
            throw new ArgumentNullException(nameof(permissions));

        // Get the set of existing and new permission IDs
        var existingPermissionIds = _userPermissions.Select(up => up.PermissionId).ToHashSet();
        var newPermissionIds = permissions.Where(p => p != null).Select(p => p.Id).ToHashSet();

        // Remove permissions that are no longer needed
        var permissionsToRemove = _userPermissions.Where(up => !newPermissionIds.Contains(up.PermissionId)).ToList();
        foreach (var userPermission in permissionsToRemove)
        {
            _userPermissions.Remove(userPermission);
        }

        // Add only new permissions that don't already exist
        foreach (var permission in permissions.Where(p => p != null && !existingPermissionIds.Contains(p.Id)))
        {
            var userPermission = new UserPermission(Id, permission.Id);
            _userPermissions.Add(userPermission);
        }

        SetLastModificationTime();
    }

    /// <summary>
    /// Sets (replaces) all roles for the user with the provided roles.
    /// Uses a diff-based approach to minimize database operations and avoid concurrency issues.
    /// </summary>
    public void SetRoles(IEnumerable<Role> roles)
    {
        if (roles == null)
            throw new ArgumentNullException(nameof(roles));

        // Get the set of existing and new role IDs
        var existingRoleIds = _userRoles.Select(ur => ur.RoleId).ToHashSet();
        var newRoleIds = roles.Where(r => r != null).Select(r => r.Id).ToHashSet();

        // Remove roles that are no longer needed
        var rolesToRemove = _userRoles.Where(ur => !newRoleIds.Contains(ur.RoleId)).ToList();
        foreach (var userRole in rolesToRemove)
        {
            _userRoles.Remove(userRole);
        }

        // Add only new roles that don't already exist
        foreach (var role in roles.Where(r => r != null && !existingRoleIds.Contains(r.Id)))
        {
            var userRole = new UserRole(Id, role.Id);
            _userRoles.Add(userRole);
        }

        SetLastModificationTime();
    }

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.Role != null && ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionName)
    {
        // Check direct permissions first
        var hasDirectPermission = _userPermissions
            .Any(up => up.Permission != null && 
                up.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));

        if (hasDirectPermission)
            return true;

        // Check permissions through roles
        return _userRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Any(rp => rp.Permission != null && 
                rp.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> GetAllPermissions()
    {
        var rolePermissions = _userRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name);

        var directPermissions = _userPermissions
            .Where(up => up.Permission != null)
            .Select(up => up.Permission!.Name);

        return rolePermissions.Concat(directPermissions).Distinct();
    }
}

