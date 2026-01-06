using AspireApp.ApiService.Infrastructure.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AspireApp.ApiService.Infrastructure.Tests.Authorization;

public class PermissionPolicyProviderTests
{
    private readonly PermissionPolicyProvider _sut;

    public PermissionPolicyProviderTests()
    {
        var options = Substitute.For<IOptions<AuthorizationOptions>>();
        options.Value.Returns(new AuthorizationOptions());
        _sut = new PermissionPolicyProvider(options);
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldReturnPermissionPolicy_WhenPolicyNameStartsWithPermission()
    {
        // Arrange
        var policyName = "Permission:Users.Read";

        // Act
        var policy = await _sut.GetPolicyAsync(policyName);

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle(r => r is PermissionRequirement);
        var requirement = (PermissionRequirement)policy.Requirements.First();
        requirement.Permissions.Should().Contain("Users.Read");
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldReturnMultiPermissionPolicy_WhenMultiplePermissionsProvided()
    {
        // Arrange
        var policyName = "Permission:Users.Read, Users.Write";

        // Act
        var policy = await _sut.GetPolicyAsync(policyName);

        // Assert
        policy.Should().NotBeNull();
        var requirement = (PermissionRequirement)policy!.Requirements.First();
        requirement.Permissions.Should().Contain(new[] { "Users.Read", "Users.Write" });
    }
}
