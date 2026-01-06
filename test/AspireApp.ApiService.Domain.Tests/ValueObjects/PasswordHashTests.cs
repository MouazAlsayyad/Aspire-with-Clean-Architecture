using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Tests.ValueObjects;

public class PasswordHashTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateInstance()
    {
        // Arrange
        var hash = "hashedPassword";
        var salt = "randomSalt";

        // Act
        var passwordHash = new PasswordHash(hash, salt);

        // Assert
        Assert.Equal(hash, passwordHash.Hash);
        Assert.Equal(salt, passwordHash.Salt);
    }

    [Theory]
    [InlineData("", "salt")]
    [InlineData(" ", "salt")]
    [InlineData(null, "salt")]
    [InlineData("hash", "")]
    [InlineData("hash", " ")]
    [InlineData("hash", null)]
    public void Constructor_WithInvalidArguments_ShouldThrowArgumentException(string? hash, string? salt)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PasswordHash(hash!, salt!));
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var ph1 = PasswordHash.Create("hash", "salt");
        var ph2 = PasswordHash.Create("hash", "salt");

        // Act & Assert
        Assert.True(ph1.Equals(ph2));
        Assert.True(ph1 == ph2);
        Assert.Equal(ph1.GetHashCode(), ph2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var ph1 = PasswordHash.Create("hash1", "salt");
        var ph2 = PasswordHash.Create("hash2", "salt");

        // Act & Assert
        Assert.False(ph1.Equals(ph2));
        Assert.True(ph1 != ph2);
    }
}
