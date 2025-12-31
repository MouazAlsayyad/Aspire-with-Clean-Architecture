using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using Role = AspireApp.ApiService.Domain.Roles.Entities.Role;

namespace AspireApp.ApiService.Domain.Users.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Role? Role { get; private set; }

    // Private constructor for EF Core
    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId cannot be empty", nameof(roleId));

        UserId = userId;
        RoleId = roleId;
    }
}

