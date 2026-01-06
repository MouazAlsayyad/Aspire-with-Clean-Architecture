using AspireApp.ApiService.Domain.Permissions.Entities;

namespace AspireApp.ApiService.Domain.Tests.Permissions.Entities;

public class PermissionTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreatePermission()
    {
        // Arrange
        var name = "User.Delete";
        var desc = "Delete User Permission";
        var res = "User";
        var act = "Delete";

        // Act
        var permission = new Permission(name, desc, res, act);

        // Assert
        Assert.Equal(name, permission.Name);
        Assert.Equal(res, permission.Resource);
        Assert.Equal(act, permission.Action);
        Assert.Equal("User.Delete", permission.GetFullPermissionName());
    }
}
