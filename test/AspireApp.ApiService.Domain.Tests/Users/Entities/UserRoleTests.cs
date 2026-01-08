using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Tests.Users.Entities;

public class UserRoleTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var userRole = new UserRole(userId, roleId);

        // Assert
        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
    }

    [Fact]
    public void UserRole_ShouldHaveNavigationProperties()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("hash", "salt"), "First", "Last");
        var role = new Role("Admin", "Admin Role", RoleType.Admin);

        // Act
        var userRole = new UserRole(user.Id, role.Id);

        // Assert
        Assert.Null(userRole.User); // Navigation properties are null until loaded
        Assert.Null(userRole.Role);
    }

    [Fact]
    public void UserRole_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var userRole = new UserRole(userId, roleId);

        // Assert
        Assert.NotEqual(Guid.Empty, userRole.Id);
        Assert.True(userRole.CreationTime <= DateTime.UtcNow);
    }
}

