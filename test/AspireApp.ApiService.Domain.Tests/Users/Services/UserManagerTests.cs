using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Common;
using NSubstitute;

namespace AspireApp.ApiService.Domain.Tests.Users.Services;

public class UserManagerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly UserManager _userManager;

    public UserManagerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _firebaseAuthService = Substitute.For<IFirebaseAuthService>();
        _userManager = new UserManager(_userRepository, _roleRepository, _permissionRepository, _firebaseAuthService);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser()
    {
        // Arrange
        var email = "test@test.com";
        var userName = "testuser";
        var passwordHash = PasswordHash.Create("hash", "salt");
        var firstName = "First";
        var lastName = "Last";

        // Act
        var result = await _userManager.CreateAsync(email, userName, passwordHash, firstName, lastName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email.ToLowerInvariant(), result.Email);
        Assert.Equal(userName, result.UserName);
    }

    [Fact]
    public async Task AssignRoleAsync_ByName_WithValidRole_ShouldAssignRole()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var roleName = "Admin";
        var role = new Role(roleName, "Admin Role", RoleType.Admin);
        _roleRepository.GetByNameAsync(roleName, Arg.Any<CancellationToken>()).Returns(role);

        // Act
        await _userManager.AssignRoleAsync(user, roleName);

        // Assert
        Assert.Contains(user.UserRoles, ur => ur.RoleId == role.Id);
    }

    [Fact]
    public async Task AssignRoleAsync_ByName_WithNonExistingRole_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var roleName = "NonExisting";
        _roleRepository.GetByNameAsync(roleName, Arg.Any<CancellationToken>()).Returns((Role?)null);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _userManager.AssignRoleAsync(user, roleName));
    }

    [Fact]
    public async Task AssignRoleAsync_ById_WithValidRole_ShouldAssignRole()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var roleId = Guid.NewGuid();
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(role);

        // Act
        await _userManager.AssignRoleAsync(user, roleId);

        // Assert
        Assert.Contains(user.UserRoles, ur => ur.RoleId == role.Id);
    }

    [Fact]
    public void RemoveRole_WithValidRole_ShouldRemoveRole()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        user.AddRole(role);
        var roleId = role.Id;

        // Act
        _userManager.RemoveRole(user, roleId);

        // Assert
        Assert.DoesNotContain(user.UserRoles, ur => ur.RoleId == roleId && !ur.IsDeleted);
    }

    [Fact]
    public void ChangePassword_WithValidPassword_ShouldUpdatePassword()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("old", "salt"), "F", "L");
        var newPasswordHash = PasswordHash.Create("new", "newsalt");

        // Act
        _userManager.ChangePassword(user, newPasswordHash);

        // Assert
        Assert.Equal(newPasswordHash.Hash, user.PasswordHash.Hash);
        Assert.Equal(newPasswordHash.Salt, user.PasswordHash.Salt);
    }

    [Fact]
    public void Activate_ShouldActivateUser()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        user.Deactivate();

        // Act
        _userManager.Activate(user);

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldDeactivateUser()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");

        // Act
        _userManager.Deactivate(user);

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void ConfirmEmail_ShouldConfirmUserEmail()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");

        // Act
        _userManager.ConfirmEmail(user);

        // Assert
        Assert.True(user.IsEmailConfirmed);
    }

    [Fact]
    public void ValidateDeletion_WithValidUser_ShouldNotThrow()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");

        // Act & Assert
        _userManager.ValidateDeletion(user);
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@test.com";
        _userRepository.ExistsAsync(email, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _userManager.EmailExistsAsync(email);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserNameExistsAsync_WithExistingUserName_ShouldReturnTrue()
    {
        // Arrange
        var userName = "testuser";
        var user = new User("test@test.com", userName, PasswordHash.Create("h", "s"), "F", "L");
        _userRepository.GetByUserNameAsync(userName, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _userManager.UserNameExistsAsync(userName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AssignPermissionAsync_ByName_WithValidPermission_ShouldAssignPermission()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var permissionName = "User.Read";
        var permission = new Permission(permissionName, "Read User", "User", "Read");
        _permissionRepository.GetByNameAsync(permissionName, Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        await _userManager.AssignPermissionAsync(user, permissionName);

        // Assert
        Assert.Contains(user.UserPermissions, up => up.PermissionId == permission.Id);
    }

    [Fact]
    public async Task SetRolesAsync_ShouldReplaceAllRoles()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var role1 = new Role("Admin", "Admin Role", RoleType.Admin);
        var role2 = new Role("Manager", "Manager Role", RoleType.Manager);
        user.AddRole(role1);

        var newRoleId = Guid.NewGuid();
        var newRole = new Role("User", "User Role", RoleType.User);
        _roleRepository.GetAsync(newRoleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(newRole);

        // Act
        await _userManager.SetRolesAsync(user, new[] { newRoleId });

        // Assert - Old roles should be marked as deleted, new role should be added
        var activeRoles = user.UserRoles.Where(ur => !ur.IsDeleted).ToList();
        Assert.Single(activeRoles);
        Assert.Equal(newRole.Id, activeRoles[0].RoleId);
    }

    [Fact]
    public async Task SetPermissionsAsync_ShouldReplaceAllPermissions()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var perm1 = new Permission("User.Read", "Read", "User", "Read");
        user.AddPermission(perm1);

        var newPermId = Guid.NewGuid();
        var newPerm = new Permission("User.Write", "Write", "User", "Write");
        _permissionRepository.GetAsync(newPermId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(newPerm);

        // Act
        await _userManager.SetPermissionsAsync(user, new[] { newPermId });

        // Assert
        var activePerms = user.UserPermissions.Where(up => !up.IsDeleted).ToList();
        Assert.Single(activePerms);
        Assert.Equal(newPerm.Id, activePerms[0].PermissionId);
    }

    [Fact]
    public void UpdateLanguage_WithValidLanguage_ShouldUpdateLanguage()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");

        // Act
        _userManager.UpdateLanguage(user, "ar");

        // Assert
        Assert.Equal("ar", user.Language);
    }

    [Fact]
    public void UpdateFcmToken_ShouldUpdateToken()
    {
        // Arrange
        var user = new User("test@test.com", "testuser", PasswordHash.Create("h", "s"), "F", "L");
        var fcmToken = "test-fcm-token";

        // Act
        _userManager.UpdateFcmToken(user, fcmToken);

        // Assert
        Assert.Equal(fcmToken, user.FcmToken);
    }
}

