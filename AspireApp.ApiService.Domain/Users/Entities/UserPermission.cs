using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Permissions.Entities;

namespace AspireApp.ApiService.Domain.Users.Entities;

public class UserPermission : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Permission? Permission { get; private set; }

    // Private constructor for EF Core
    private UserPermission() { }

    public UserPermission(Guid userId, Guid permissionId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId cannot be empty", nameof(permissionId));

        UserId = userId;
        PermissionId = permissionId;
    }
}

