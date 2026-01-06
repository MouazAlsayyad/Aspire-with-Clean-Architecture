using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class RoleRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly RoleRepository _sut;

    public RoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new RoleRepository(_context);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnRole_WhenNameMatches()
    {
        // Arrange
        var name = "Admin";
        var role = new Role(name, "Administrator role", RoleType.Admin);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByNameAsync(name);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
    }

    [Fact]
    public async Task GetAsync_ShouldIncludePermissions()
    {
        // Arrange
        var role = new Role("Admin", "Administrator role", RoleType.Admin);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.RolePermissions.Should().NotBeNull();
    }
}
