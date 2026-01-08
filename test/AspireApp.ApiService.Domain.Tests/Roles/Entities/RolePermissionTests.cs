using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;

namespace AspireApp.ApiService.Domain.Tests.Roles.Entities;

public class RolePermissionTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRolePermission()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var rolePermission = new RolePermission(roleId, permissionId);

        // Assert
        Assert.Equal(roleId, rolePermission.RoleId);
        Assert.Equal(permissionId, rolePermission.PermissionId);
    }

    [Fact]
    public void RolePermission_ShouldHaveNavigationProperties()
    {
        // Arrange
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        var permission = new Permission("User.Read", "Read User", "User", "Read");

        // Act
        var rolePermission = new RolePermission(role.Id, permission.Id);

        // Assert
        Assert.Null(rolePermission.Role); // Navigation properties are null until loaded
        Assert.Null(rolePermission.Permission);
    }

    [Fact]
    public void RolePermission_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var rolePermission = new RolePermission(roleId, permissionId);

        // Assert
        Assert.NotEqual(Guid.Empty, rolePermission.Id);
        Assert.True(rolePermission.CreationTime <= DateTime.UtcNow);
    }
}

