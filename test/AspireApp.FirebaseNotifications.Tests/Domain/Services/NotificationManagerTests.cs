using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Domain.Events;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Services;
using FluentAssertions;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Domain.Services;

public class NotificationManagerTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
    private readonly NotificationManager _notificationManager;

    public NotificationManagerTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _notificationManager = new NotificationManager(
            _notificationRepositoryMock.Object,
            _domainEventDispatcherMock.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_ShouldCreateAndPublishEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Test Title";
        var message = "Test Message";

        // Act
        var result = await _notificationManager.CreateNotificationAsync(
            NotificationType.Info,
            NotificationPriority.Normal,
            title,
            "Title Ar",
            message,
            "Message Ar",
            userId
        );

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.DomainEvents.Should().Contain(e => e is NotificationCreatedEvent);

        _notificationRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNotificationStatusAsync_ShouldUpdateStatus_WhenNotificationExists()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "Ta", "M", "Ma", Guid.NewGuid());
        // Reflection to set ID if needed, or assume repository returns it matching ID
        // Entity usually has protected/private setter provided by base class or it's new.
        // EF Core sets it.
        // We can mock repository to return this notification.

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, default, default))
            .ReturnsAsync(notification);

        // Act
        await _notificationManager.UpdateNotificationStatusAsync(notificationId, isRead: true);

        // Assert
        notification.IsRead.Should().BeTrue();
        _notificationRepositoryMock.Verify(x => x.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldMarkUnread_WhenExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var note1 = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "Ta", "M", "Ma", userId);
        var note2 = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "Ta", "M", "Ma", userId);

        _notificationRepositoryMock.Setup(x => x.GetUnreadNotificationsAsync(userId, default))
            .ReturnsAsync(new List<Notification> { note1, note2 });

        // Act
        var count = await _notificationManager.MarkAllAsReadAsync(userId);

        // Assert
        count.Should().Be(2);
        note1.IsRead.Should().BeTrue();
        note2.IsRead.Should().BeTrue();
        _notificationRepositoryMock.Verify(x => x.UpdateManyAsync(It.IsAny<IEnumerable<Notification>>(), default, default), Times.Once);
    }
}
