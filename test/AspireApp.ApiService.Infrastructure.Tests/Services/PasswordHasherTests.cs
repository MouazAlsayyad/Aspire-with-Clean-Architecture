using AspireApp.ApiService.Infrastructure.Services;
using FluentAssertions;

namespace AspireApp.ApiService.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut;

    public PasswordHasherTests()
    {
        _sut = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashAndSalt_WhenPasswordIsProvided()
    {
        // Arrange
        var password = "StrongPassword123!";

        // Act
        var (hash, salt) = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        salt.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void HashPassword_ShouldThrowArgumentException_WhenPasswordIsEmpty()
    {
        // Act
        var act = () => _sut.HashPassword("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        var password = "StrongPassword123!";
        var (hash, salt) = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var password = "StrongPassword123!";
        var wrongPassword = "WrongPassword";
        var (hash, salt) = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "hash", "salt")]
    [InlineData("password", "", "salt")]
    [InlineData("password", "hash", "")]
    public void VerifyPassword_ShouldReturnFalse_WhenInputIsInvalid(string password, string hash, string salt)
    {
        // Act
        var result = _sut.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenSaltIsInvalidBase64()
    {
        // Act
        var result = _sut.VerifyPassword("password", "hash", "invalid-base64");

        // Assert
        result.Should().BeFalse();
    }
}
