using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailMatches()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User(email, "testuser", new PasswordHash("hash", "salt"), "FirstName", "LastName");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email.ToLowerInvariant());
    }

    [Fact]
    public async Task GetByUserNameAsync_ShouldReturnUser_WhenUserNameMatches()
    {
        // Arrange
        var userName = "testuser";
        var user = new User("test@example.com", userName, new PasswordHash("hash", "salt"), "FirstName", "LastName");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUserNameAsync(userName);

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User(email, "testuser", new PasswordHash("hash", "salt"), "FirstName", "LastName");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }
}
