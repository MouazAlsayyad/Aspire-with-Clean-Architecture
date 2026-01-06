using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests
{
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _sut;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var role = new Role("Admin", "Admin role", RoleType.Admin);
        await _context.Roles.AddAsync(role);

        // Act
        var result = await _sut.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        var savedRole = await _context.Roles.FindAsync(role.Id);
        savedRole.Should().NotBeNull();
    }

    [Fact]
    public void GetRepository_ShouldReturnRepositoryForType()
    {
        // Act
        var repository = _sut.GetRepository<Role>();

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<Repository<Role>>();
    }

    [Fact]
    public void GetRepository_ShouldReturnSameInstance_WhenCalledMultipleTimesForSameType()
    {
        // Act
        var repo1 = _sut.GetRepository<Role>();
        var repo2 = _sut.GetRepository<Role>();

        // Assert
        repo1.Should().BeSameAs(repo2);
    }
}
