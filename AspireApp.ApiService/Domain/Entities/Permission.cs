namespace AspireApp.ApiService.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty; // e.g., "Weather", "User", "Role"
    public string Action { get; private set; } = string.Empty; // e.g., "Read", "Write", "Delete"

    // Navigation property for many-to-many relationship
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

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

