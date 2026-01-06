using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Tests.Repositories;

public class ActivityLogRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly ActivityLogRepository _sut;

    public ActivityLogRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new ActivityLogRepository(_context);
    }

    [Fact]
    public async Task GetUserActivitiesAsync_ShouldReturnLogsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var log1 = new ActivityLog("Login", "User logged in", userId, "User1", null, null, null, null, "127.0.0.1", "Web");
        var log2 = new ActivityLog("Logout", "User logged out", userId, "User1", null, null, null, null, "127.0.0.1", "Web");
        var log3 = new ActivityLog("Other", "Other action", Guid.NewGuid(), "User2", null, null, null, null, "127.0.0.1", "Web");

        _context.ActivityLogs.AddRange(log1, log2, log3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserActivitiesAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.UserId.Should().Be(userId));
    }
}
