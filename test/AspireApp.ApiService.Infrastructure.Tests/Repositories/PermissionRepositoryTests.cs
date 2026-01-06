using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class PermissionRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionRepository _sut;

    public PermissionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new PermissionRepository(_context);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnPermission_WhenNameMatches()
    {
        // Arrange
        var name = "Users.Create";
        var permission = new Permission(name, "Create users permission", "Users", "Create");
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByNameAsync(name);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
    }

    [Fact]
    public async Task GetByResourceAsync_ShouldReturnPermissions_WhenResourceMatches()
    {
        // Arrange
        var resource = "Users";
        var permission1 = new Permission("Users.Create", "Create users", resource, "Create");
        var permission2 = new Permission("Users.Read", "Read users", resource, "Read");
        _context.Permissions.AddRange(permission1, permission2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByResourceAsync(resource);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Resource.Should().Be(resource));
    }

    [Fact]
    public async Task GetListAsync_ShouldIncludeResource()
    {
        // Arrange
        var permission = new Permission("Users.Read", "Read users", "Users", "Read");
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetListAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.First().Resource.Should().Be("Users");
    }
}
