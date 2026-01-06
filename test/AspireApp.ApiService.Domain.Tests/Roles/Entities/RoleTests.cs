using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;

namespace AspireApp.ApiService.Domain.Tests.Roles.Entities;

public class RoleTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateRole()
    {
        // Arrange
        var name = "Manager";
        var description = "Manager Role";
        var type = RoleType.Manager;

        // Act
        var role = new Role(name, description, type);

        // Assert
        Assert.Equal(name, role.Name);
        Assert.Equal(description, role.Description);
        Assert.Equal(type, role.Type);
    }

    [Fact]
    public void AddPermission_ShouldAddPermission()
    {
        // Arrange
        var role = new Role("Role", "Desc", RoleType.Manager);
        var permission = new Permission("Perm", "Desc", "Res", "Act");

        // Act
        role.AddPermission(permission);

        // Assert
        Assert.Contains(role.RolePermissions, rp => rp.PermissionId == permission.Id);
    }
}
