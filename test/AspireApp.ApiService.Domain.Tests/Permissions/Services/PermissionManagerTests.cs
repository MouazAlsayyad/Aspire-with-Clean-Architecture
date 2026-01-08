using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Services;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using NSubstitute;

namespace AspireApp.ApiService.Domain.Tests.Permissions.Services;

public class PermissionManagerTests
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository;
    private readonly PermissionManager _permissionManager;

    public PermissionManagerTests()
    {
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _rolePermissionRepository = Substitute.For<IRepository<RolePermission>>();
        _permissionManager = new PermissionManager(_permissionRepository, _rolePermissionRepository);
    }

    [Fact]
    public async Task CreateAsync_WithUniquePermission_ShouldCreatePermission()
    {
        // Arrange
        var name = "User.Create";
        var description = "Create User";
        var resource = "User";
        var action = "Create";
        _permissionRepository.GetByNameAsync(name, Arg.Any<CancellationToken>()).Returns((Permission?)null);

        // Act
        var result = await _permissionManager.CreateAsync(name, description, resource, action);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);
        Assert.Equal(resource, result.Resource);
        Assert.Equal(action, result.Action);
    }

    [Fact]
    public async Task CreateAsync_WithExistingPermissionName_ShouldThrowDomainException()
    {
        // Arrange
        var name = "User.Create";
        var existingPermission = new Permission(name, "Existing", "User", "Create");
        _permissionRepository.GetByNameAsync(name, Arg.Any<CancellationToken>()).Returns(existingPermission);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _permissionManager.CreateAsync(name, "New", "User", "Create"));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task ValidateDeletionAsync_WithUnusedPermission_ShouldNotThrow()
    {
        // Arrange
        var permission = new Permission("User.Delete", "Delete User", "User", "Delete");
        _rolePermissionRepository.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        await _permissionManager.ValidateDeletionAsync(permission);
    }

    [Fact]
    public async Task ValidateDeletionAsync_WithUsedPermission_ShouldThrowDomainException()
    {
        // Arrange
        var permission = new Permission("User.Delete", "Delete User", "User", "Delete");
        _rolePermissionRepository.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _permissionManager.ValidateDeletionAsync(permission));
        Assert.Contains("cannot be deleted", exception.Message);
    }

    [Fact]
    public async Task ValidateDeletionAsync_WithNullPermission_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _permissionManager.ValidateDeletionAsync(null!));
    }

    [Fact]
    public async Task PermissionNameExistsAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var name = "User.Read";
        var permission = new Permission(name, "Read User", "User", "Read");
        _permissionRepository.GetByNameAsync(name, Arg.Any<CancellationToken>()).Returns(permission);

        // Act
        var result = await _permissionManager.PermissionNameExistsAsync(name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PermissionNameExistsAsync_WithNonExistingName_ShouldReturnFalse()
    {
        // Arrange
        var name = "User.NonExisting";
        _permissionRepository.GetByNameAsync(name, Arg.Any<CancellationToken>()).Returns((Permission?)null);

        // Act
        var result = await _permissionManager.PermissionNameExistsAsync(name);

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
        var result = await _permissionManager.PermissionExistsAsync(permissionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PermissionExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        _permissionRepository.GetAsync(permissionId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Permission?)null);

        // Act
        var result = await _permissionManager.PermissionExistsAsync(permissionId);

        // Assert
        Assert.False(result);
    }
}

