using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Application.Users.Validators;
using AspireApp.ApiService.Domain.Permissions.Services;
using AspireApp.ApiService.Domain.Roles.Services;
using AspireApp.ApiService.Domain.Users.Services;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Users.Validators;

public class UserValidatorTests
{
    private readonly IUserManager _userManager;
    private readonly IRoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;

    public UserValidatorTests()
    {
        _userManager = Substitute.For<IUserManager>();
        _roleManager = Substitute.For<IRoleManager>();
        _permissionManager = Substitute.For<IPermissionManager>();
    }

    [Fact]
    public async Task UpdateUserRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new UpdateUserRequestValidator();
        var request = new UpdateUserRequest("UpdateFirst", "UpdateLast", true);

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ToggleUserActivationRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new ToggleUserActivationRequestValidator();
        var request = new ToggleUserActivationRequest(true);

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdatePasswordRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new UpdatePasswordRequestValidator();
        var request = new UpdatePasswordRequest("OldPass123!", "NewPass123!");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AssignRoleToUserRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new AssignRoleToUserRequestValidator(_roleManager);
        var roleId = Guid.NewGuid();
        var request = new AssignRoleToUserRequest(new List<Guid> { roleId });
        _roleManager.RoleExistsAsync(roleId).Returns(true);

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AssignPermissionsToUserRequestValidator_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new AssignPermissionsToUserRequestValidator(_permissionManager);
        var permissionId = Guid.NewGuid();
        var request = new AssignPermissionsToUserRequest(new List<Guid> { permissionId });
        _permissionManager.PermissionExistsAsync(permissionId).Returns(true);

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
