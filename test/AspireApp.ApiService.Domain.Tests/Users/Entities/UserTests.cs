using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Tests.Users.Entities;

public class UserTests
{
    private PasswordHash CreateDefaultPasswordHash() => PasswordHash.Create("hash", "salt");

    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var userName = "testuser";
        var passwordHash = CreateDefaultPasswordHash();
        var firstName = "First";
        var lastName = "Last";

        // Act
        var user = new User(email, userName, passwordHash, firstName, lastName);

        // Assert
        Assert.Equal(email.ToLowerInvariant(), user.Email);
        Assert.Equal(userName, user.UserName);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.True(user.IsActive);
        Assert.False(user.IsEmailConfirmed);
    }

    [Fact]
    public void UpdateLanguage_WithValidLanguage_ShouldUpdate()
    {
        // Arrange
        var user = new User("t@e.com", "u", CreateDefaultPasswordHash(), "F", "L");

        // Act
        user.UpdateLanguage("ar");

        // Assert
        Assert.Equal("ar", user.Language);
    }

    [Fact]
    public void UpdateLanguage_WithInvalidLanguage_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User("t@e.com", "u", CreateDefaultPasswordHash(), "F", "L");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.UpdateLanguage("fr"));
    }

    [Fact]
    public void RolesAndPermissions_Management_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User("t@e.com", "u", CreateDefaultPasswordHash(), "F", "L");
        var role = new Role("Admin", "Admin Role", AspireApp.ApiService.Domain.Roles.Enums.RoleType.Manager);
        var permission = new Permission("User.Read", "Read User", "User", "Read");

        // Act
        user.AddRole(role);
        user.AddPermission(permission);

        // Assert
        Assert.Contains(user.UserRoles, ur => ur.RoleId == role.Id);
        Assert.Contains(user.UserPermissions, up => up.PermissionId == permission.Id);
    }
}
