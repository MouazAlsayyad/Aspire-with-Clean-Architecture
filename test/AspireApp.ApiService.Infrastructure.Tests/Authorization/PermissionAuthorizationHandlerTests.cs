using AspireApp.ApiService.Infrastructure.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspireApp.ApiService.Infrastructure.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _sut;

    public PermissionAuthorizationHandlerTests()
    {
        _sut = new PermissionAuthorizationHandler();
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenUserHasRequiredPermission()
    {
        // Arrange
        var permission = "Users.Read";
        var requirement = new PermissionRequirement(permission);
        var claims = new[] { new Claim("Permission", permission) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSucceed_WhenUserDoesNotHavePermission()
    {
        // Arrange
        var requirement = new PermissionRequirement("Users.Read");
        var claims = new[] { new Claim("Permission", "Other.Permission") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenUserHasAnyOfRequiredPermissions()
    {
        // Arrange
        var requirement = new PermissionRequirement("Users.Read", "Users.Write");
        var claims = new[] { new Claim("Permission", "Users.Write") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}
