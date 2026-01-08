
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Infrastructure.Repositories;

public class NotificationRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IDomainEventDispatcher> _dispatcherMock;

    public NotificationRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dispatcherMock = new Mock<IDomainEventDispatcher>();
    }

    [Fact]
    public async Task GetNotificationsAsync_ShouldReturnNotificationsForUser()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options, _dispatcherMock.Object);
        var repository = new NotificationRepository(context);

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var note1 = new Notification(NotificationType.Info, NotificationPriority.Normal, "T1", "Ta1", "M1", "Ma1", userId);
        var note2 = new Notification(NotificationType.Warning, NotificationPriority.High, "T2", "Ta2", "M2", "Ma2", userId);
        var noteOther = new Notification(NotificationType.Info, NotificationPriority.Normal, "T3", "Ta3", "M3", "Ma3", otherUserId);

        // We need to add notifications to the context.
        // Since Notification is in a separate assembly, ApplicationDbContext might not automatically include it in default Sets 
        // unless OnModelCreating loads it effectively.
        // In ApplicationDbContext.OnModelCreating, it loads assemblies.
        // But in Unit Test, effectively loaded assemblies might differ.
        // However, we can use context.Set<Notification>().Add(...) directly.
        
        context.Set<Notification>().Add(note1);
        context.Set<Notification>().Add(note2);
        context.Set<Notification>().Add(noteOther);
        await context.SaveChangesAsync();

        // Act
        var (notifications, hasMore) = await repository.GetNotificationsAsync(userId, pageSize: 10);

        // Assert
        notifications.Should().HaveCount(2);
        notifications.Should().Contain(n => n.Title == "T1");
        notifications.Should().Contain(n => n.Title == "T2");
        hasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options, _dispatcherMock.Object);
        var repository = new NotificationRepository(context);

        var userId = Guid.NewGuid();
        var note1 = new Notification(NotificationType.Info, NotificationPriority.Normal, "T1", "Ta1", "M1", "Ma1", userId);
        var note2 = new Notification(NotificationType.Info, NotificationPriority.Normal, "T2", "Ta2", "M2", "Ma2", userId);
        note2.MarkAsRead(); // Read
        
        context.Set<Notification>().Add(note1);
        context.Set<Notification>().Add(note2);
        await context.SaveChangesAsync();

        // Act
        var count = await repository.GetUnreadCountAsync(userId);

        // Assert
        count.Should().Be(1);
    }
}
