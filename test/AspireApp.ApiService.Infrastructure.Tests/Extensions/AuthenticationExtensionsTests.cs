using AspireApp.ApiService.Infrastructure.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.ApiService.Infrastructure.Tests.Extensions;

public class AuthenticationExtensionsTests
{
    [Fact]
    public void AddJwtAuthentication_ShouldRegisterAuthenticationServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"Jwt:SecretKey", "super_secret_key_12345678901234567890"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        }).Build();

        // Act
        services.AddJwtAuthentication(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IAuthenticationSchemeProvider>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterAuthorizationPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAuthorization(); // Required base

        // Act
        services.AddRoleBasedAuthorization();
        var provider = services.BuildServiceProvider();

        // Assert
        var policyProvider = provider.GetService<IAuthorizationPolicyProvider>();
        policyProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddPermissionBasedAuthorization_ShouldRegisterPermissionServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAuthorization(); // Required for IOptions<AuthorizationOptions>

        // Act
        services.AddPermissionBasedAuthorization();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IAuthorizationPolicyProvider>().Should().BeOfType<AspireApp.ApiService.Infrastructure.Authorization.PermissionPolicyProvider>();
        provider.GetService<IAuthorizationHandler>().Should().BeOfType<AspireApp.ApiService.Infrastructure.Authorization.PermissionAuthorizationHandler>();
    }
}
