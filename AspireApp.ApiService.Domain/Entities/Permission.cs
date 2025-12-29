using AspireApp.ApiService.Domain.Common;

namespace AspireApp.ApiService.Domain.Entities;

/// <summary>
/// Permission aggregate root.
/// Permission is the entry point to the Permission aggregate.
/// </summary>
public class Permission : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty; // e.g., "Weather", "User", "Role"
    public string Action { get; private set; } = string.Empty; // e.g., "Read", "Write", "Delete"

    // Navigation property for many-to-many relationship with roles
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    // Navigation property for many-to-many relationship with users (direct assignment)
    private readonly List<UserPermission> _userPermissions = [];
    public IReadOnlyCollection<UserPermission> UserPermissions => _userPermissions.AsReadOnly();

    // Private constructor for EF Core
    private Permission() { }

    public Permission(string name, string description, string resource, string action)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Resource cannot be empty", nameof(resource));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        Name = name;
        Description = description ?? string.Empty;
        Resource = resource;
        Action = action;
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
        SetLastModificationTime();
    }

    public string GetFullPermissionName() => $"{Resource}.{Action}";
}

