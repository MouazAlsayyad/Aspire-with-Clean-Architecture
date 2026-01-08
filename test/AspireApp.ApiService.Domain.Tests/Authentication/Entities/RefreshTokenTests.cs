using AspireApp.ApiService.Domain.Authentication.Entities;

namespace AspireApp.ApiService.Domain.Tests.Authentication.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Assert
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(token, refreshToken.Token);
        Assert.Equal(expiresAt, refreshToken.ExpiresAt);
        Assert.False(refreshToken.IsRevoked);
        Assert.Null(refreshToken.RevokedAt);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.Empty;
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RefreshToken(userId, token, expiresAt));
        Assert.Contains("UserId", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullToken_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RefreshToken(userId, null!, expiresAt));
        Assert.Contains("Token", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyToken_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RefreshToken(userId, token, expiresAt));
        Assert.Contains("Token", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceToken_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "   ";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RefreshToken(userId, token, expiresAt));
        Assert.Contains("Token", exception.Message);
    }

    [Fact]
    public void Revoke_ShouldSetIsRevokedToTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Act
        refreshToken.Revoke();

        // Assert
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(userId, token, expiresAt);
        var before = DateTime.UtcNow;

        // Act
        refreshToken.Revoke();
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(refreshToken.RevokedAt);
        Assert.True(refreshToken.RevokedAt >= before);
        Assert.True(refreshToken.RevokedAt <= after);
    }

    [Fact]
    public void IsExpired_WithFutureExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Act
        var isExpired = refreshToken.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsExpired_WithPastExpiryDate_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(-1);
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Act
        var isExpired = refreshToken.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void IsValid_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Act
        var isValid = refreshToken.IsValid;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(userId, token, expiresAt);
        refreshToken.Revoke();

        // Act
        var isValid = refreshToken.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(-1);
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Act
        var isValid = refreshToken.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void RefreshToken_ShouldInheritFromBaseEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Assert
        Assert.NotEqual(Guid.Empty, refreshToken.Id);
        Assert.True(refreshToken.CreationTime <= DateTime.UtcNow);
    }
}

