namespace AspireApp.ApiService.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    // Navigation properties
    public Role? Role { get; private set; }
    public Permission? Permission { get; private set; }

    // Private constructor for EF Core
    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId cannot be empty", nameof(roleId));
        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId cannot be empty", nameof(permissionId));

        RoleId = roleId;
        PermissionId = permissionId;
    }
}

