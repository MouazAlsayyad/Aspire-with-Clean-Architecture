using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Enums;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class RepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<Role> _sut;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new Repository<Role>(_context);
    }

    [Fact]
    public async Task InsertAsync_ShouldAddEntityToContext()
    {
        // Arrange
        var role = new Role("Admin", "Administrator role", RoleType.Admin);

        // Act
        await _sut.InsertAsync(role, autoSave: true);

        // Assert
        var result = await _context.Roles.FindAsync(role.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var role = new Role("User", "Standard user role", RoleType.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var role1 = new Role("Role1", "Description1", RoleType.User);
        var role2 = new Role("Role2", "Description2", RoleType.User);
        var deletedRole = new Role("Role3", "Description3", RoleType.User);
        deletedRole.Delete();

        await _context.Roles.AddRangeAsync(role1, role2, deletedRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Role1");
        result.Should().Contain(r => r.Name == "Role2");
        result.Should().NotContain(r => r.Name == "Role3");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        // Arrange
        var role = new Role("DeleteMe", "To be deleted", RoleType.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(role, autoSave: true);

        // Assert
        var result = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == role.Id);
        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task HardDeleteAsync_ShouldRemoveEntityFromDatabase()
    {
        // Arrange
        var role = new Role("HardDeleteMe", "To be removed", RoleType.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        await _sut.HardDeleteAsync(role, autoSave: true);

        // Assert
        var result = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == role.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        // Arrange
        var role = new Role("ExistCheck", "Checking existence", RoleType.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsAsync(role.Id);

        // Assert
        result.Should().BeTrue();
    }
}
