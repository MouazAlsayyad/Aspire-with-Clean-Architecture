using AspireApp.ApiService.Domain.Authentication.Entities;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class RefreshTokenRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new RefreshTokenRepository(_context);
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnToken_WhenTokenMatches()
    {
        // Arrange
        var tokenValue = "valid-token";
        var user = new AspireApp.ApiService.Domain.Users.Entities.User("test@test.com", "test", new AspireApp.ApiService.Domain.ValueObjects.PasswordHash("hash", "salt"), "Test", "User");
        SetId(user, Guid.NewGuid());
        _context.Users.Add(user);

        var refreshToken = new RefreshToken(user.Id, tokenValue, DateTime.UtcNow.AddDays(7));
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByTokenAsync(tokenValue);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenValue);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_ShouldRevokeTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token1 = new RefreshToken(userId, "token1", DateTime.UtcNow.AddDays(7));
        var token2 = new RefreshToken(userId, "token2", DateTime.UtcNow.AddDays(7));
        _context.RefreshTokens.AddRange(token1, token2);
        await _context.SaveChangesAsync();

        // Act
        await _sut.RevokeAllUserTokensAsync(userId);
        await _context.SaveChangesAsync();

        // Assert
        var remaining = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        remaining.Should().HaveCount(2);
        remaining.Should().OnlyContain(t => t.IsRevoked);
    }

    private void SetId(BaseEntity entity, Guid id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(entity, id);
        }
    }
}
