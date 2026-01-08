using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Tests.Users.Entities;

public class UserPermissionTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserPermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var userPermission = new UserPermission(userId, permissionId);

        // Assert
        Assert.Equal(userId, userPermission.UserId);
        Assert.Equal(permissionId, userPermission.PermissionId);
    }

    [Fact]
    public void UserPermission_ShouldHaveNavigationProperties()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");
        var permission = new Permission("User.Read", "Read User", "User", "Read");

        // Act
        var userPermission = new UserPermission(user.Id, permission.Id);

        // Assert
        Assert.Null(userPermission.User); // Navigation properties are null until loaded
        Assert.Null(userPermission.Permission);
    }

    [Fact]
    public void UserPermission_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var userPermission = new UserPermission(userId, permissionId);

        // Assert
        Assert.NotEqual(Guid.Empty, userPermission.Id);
        Assert.True(userPermission.CreationTime <= DateTime.UtcNow);
    }
}

