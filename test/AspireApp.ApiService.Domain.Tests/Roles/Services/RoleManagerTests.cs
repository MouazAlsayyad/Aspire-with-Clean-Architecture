using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Roles.Services;
using AspireApp.Domain.Shared.Common;
using NSubstitute;

namespace AspireApp.ApiService.Domain.Tests.Roles.Services;

public class RoleManagerTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly RoleManager _roleManager;

    public RoleManagerTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _roleManager = new RoleManager(_roleRepository, _permissionRepository);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRole()
    {
        // Arrange
        var name = "Manager";
        var description = "Manager Role";
        var type = RoleType.Manager;

        // Act
        var result = await _roleManager.CreateAsync(name, description, type);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);
        Assert.Equal(type, result.Type);
    }

    [Fact]
    public void UpdateDescription_WithValidRole_ShouldUpdateDescription()
    {
        // Arrange
        var role = new Role("Admin", "Old Description", RoleType.Admin);
        var newDescription = "New Description";

        // Act
        _roleManager.UpdateDescription(role, newDescription);

        // Assert
        Assert.Equal(newDescription, role.Description);
    }

    [Fact]
    public void UpdateDescription_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _roleManager.UpdateDescription(null!, "Description"));
    }

    [Fact]
    public async Task AddPermissionAsync_ById_WithValidPermission_ShouldAddPermission()
    {
        // Arrange
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        var permissionId = Guid.NewGuid();
        var permission = new Permission("User.Read", "Read User", "User", "Read");
        _permissionRepository.GetAsync(permissionId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        await _roleManager.AddPermissionAsync(role, permissionId);

        // Assert
        Assert.Contains(role.RolePermissions, rp => rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task AddPermissionAsync_ById_WithNonExistingPermission_ShouldThrowDomainException()
    {
        // Arrange
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        var permissionId = Guid.NewGuid();
        _permissionRepository.GetAsync(permissionId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Permission?)null);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _roleManager.AddPermissionAsync(role, permissionId));
    }

    [Fact]
    public async Task AddPermissionAsync_ByName_WithValidPermission_ShouldAddPermission()
    {
        // Arrange
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        var permissionName = "User.Read";
        var permission = new Permission(permissionName, "Read User", "User", "Read");
        _permissionRepository.GetByNameAsync(permissionName, Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        await _roleManager.AddPermissionAsync(role, permissionName);

        // Assert
        Assert.Contains(role.RolePermissions, rp => rp.PermissionId == permission.Id);
    }

    [Fact]
    public void RemovePermission_WithValidPermission_ShouldRemovePermission()
    {
        // Arrange
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        var permission = new Permission("User.Read", "Read User", "User", "Read");
        role.AddPermission(permission);
        var permissionId = permission.Id;

        // Act
        _roleManager.RemovePermission(role, permissionId);

        // Assert
        Assert.DoesNotContain(role.RolePermissions, rp => rp.PermissionId == permissionId && !rp.IsDeleted);
    }

    [Fact]
    public void ValidateDeletion_WithNonAdminRole_ShouldNotThrow()
    {
        // Arrange
        var role = new Role("Manager", "Manager Role", RoleType.Manager);

        // Act & Assert
        _roleManager.ValidateDeletion(role);
    }

    [Fact]
    public void ValidateDeletion_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _roleManager.ValidateDeletion(null!));
    }

    [Fact]
    public async Task RoleNameExistsAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var name = "Admin";
        var role = new Role(name, "Admin Role", RoleType.Admin);
        _roleRepository.GetByNameAsync(name, Arg.Any<CancellationToken>()).Returns(role);

        // Act
        var result = await _roleManager.RoleNameExistsAsync(name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RoleNameExistsAsync_WithNonExistingName_ShouldReturnFalse()
    {
        // Arrange
        var name = "NonExisting";
        _roleRepository.GetByNameAsync(name, Arg.Any<CancellationToken>()).Returns((Role?)null);

        // Act
        var result = await _roleManager.RoleNameExistsAsync(name);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PermissionExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var permission = new Permission("User.Read", "Read User", "User", "Read");
        _permissionRepository.GetAsync(permissionId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        var result = await _roleManager.PermissionExistsAsync(permissionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RoleExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role("Admin", "Admin Role", RoleType.Admin);
        _roleRepository.GetAsync(roleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(role);

        // Act
        var result = await _roleManager.RoleExistsAsync(roleId);

        // Assert
        Assert.True(result);
    }
}

