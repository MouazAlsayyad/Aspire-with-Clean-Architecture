using AspireApp.ApiService.Domain.Enums;

namespace AspireApp.ApiService.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RoleType Type { get; private set; }

    // Navigation property for many-to-many relationship
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Navigation property for permissions
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    // Private constructor for EF Core
    private Role() { }

    public Role(string name, string description, RoleType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Type = type;
    }

    public void UpdateDescription(string description, Guid? modifiedBy = null)
    {
        Description = description ?? string.Empty;
        SetLastModificationTime(modifiedBy: modifiedBy);
    }

    public void AddPermission(Permission permission, Guid? modifiedBy = null)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));
    
        if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return; // Permission already assigned

        var rolePermission = new RolePermission(Id, permission.Id);
        _rolePermissions.Add(rolePermission);
        SetLastModificationTime(modifiedBy: modifiedBy);
    }

    public void RemovePermission(Guid permissionId, Guid? modifiedBy = null)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
            SetLastModificationTime(modifiedBy: modifiedBy);
        }
    }

    public bool HasPermission(string permissionName)
    {
        return _rolePermissions.Any(rp => 
            rp.Permission != null && 
            rp.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }
}

