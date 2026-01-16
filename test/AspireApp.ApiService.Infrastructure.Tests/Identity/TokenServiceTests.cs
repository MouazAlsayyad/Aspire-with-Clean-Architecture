using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.ApiService.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace AspireApp.ApiService.Infrastructure.Tests.Identity;

public class TokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly TokenService _sut;
    private const string SecretKey = "super_secret_key_12345678901234567890";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    public TokenServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();

        var secretSection = Substitute.For<IConfigurationSection>();
        secretSection.Value.Returns(SecretKey);
        _configuration.GetSection("Jwt:SecretKey").Returns(secretSection);
        _configuration["Jwt:SecretKey"].Returns(SecretKey);

        var issuerSection = Substitute.For<IConfigurationSection>();
        issuerSection.Value.Returns(Issuer);
        _configuration.GetSection("Jwt:Issuer").Returns(issuerSection);
        _configuration["Jwt:Issuer"].Returns(Issuer);

        var audienceSection = Substitute.For<IConfigurationSection>();
        audienceSection.Value.Returns(Audience);
        _configuration.GetSection("Jwt:Audience").Returns(audienceSection);
        _configuration["Jwt:Audience"].Returns(Audience);

        var expirationSection = Substitute.For<IConfigurationSection>();
        expirationSection.Value.Returns("60");
        _configuration.GetSection("Jwt:ExpirationMinutes").Returns(expirationSection);
        _configuration["Jwt:ExpirationMinutes"].Returns("60");


        _sut = new TokenService(_configuration);
    }

    private User CreateUser(Guid? id = null)
    {
        var passwordHash = new PasswordHash("hash", "salt");
        var user = new User("test@example.com", "testuser", passwordHash, "Test", "User");

        if (id.HasValue)
        {
            // Use reflection in case Id setter is protected/private
            var prop = typeof(AspireApp.Domain.Shared.Entities.BaseEntity).GetProperty("Id");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(user, id.Value);
            }
        }

        return user;
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnToken_WhenValidUserProvided()
    {
        // Arrange
        var user = CreateUser(Guid.NewGuid());

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRandomString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateToken_ShouldReturnTrue_ForValidToken()
    {
        // Arrange
        var user = CreateUser(Guid.NewGuid());
        var token = _sut.GenerateAccessToken(user);

        // Act
        var result = _sut.ValidateToken(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_ShouldReturnFalse_ForInvalidToken()
    {
        // Act
        var result = _sut.ValidateToken("invalid_token_string");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetUserIdFromToken_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        var token = _sut.GenerateAccessToken(user);

        // Act
        var result = _sut.GetUserIdFromToken(token);

        // Assert
        result.Should().Be(userId);
    }
}
