
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using FluentAssertions;

namespace AspireApp.FirebaseNotifications.Tests.Domain.Entities;

public class NotificationTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenValidArgumentsProvided()
    {
        // Arrange
        var type = NotificationType.Info;
        var priority = NotificationPriority.High;
        var title = "Test Title";
        var titleAr = "Test Title Ar";
        var message = "Test Message";
        var messageAr = "Test Message Ar";
        var userId = Guid.NewGuid();
        var actionUrl = "http://example.com";

        // Act
        var notification = new Notification(type, priority, title, titleAr, message, messageAr, userId, actionUrl);

        // Assert
        notification.Type.Should().Be(type);
        notification.Priority.Should().Be(priority);
        notification.Title.Should().Be(title);
        notification.TitleAr.Should().Be(titleAr);
        notification.Message.Should().Be(message);
        notification.MessageAr.Should().Be(messageAr);
        notification.UserId.Should().Be(userId);
        notification.ActionUrl.Should().Be(actionUrl);
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowArgumentException_WhenTitleIsInvalid(string? invalidTitle)
    {
        // Act
        Action act = () => new Notification(
            NotificationType.Info,
            NotificationPriority.High,
            invalidTitle!,
            "Title Ar",
            "Message",
            "Message Ar",
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Title cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowArgumentException_WhenMessageIsInvalid(string? invalidMessage)
    {
        // Act
        Action act = () => new Notification(
            NotificationType.Info,
            NotificationPriority.High,
            "Title",
            "Title Ar",
            invalidMessage!,
            "Message Ar",
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Message cannot be empty*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        // Act
        Action act = () => new Notification(
            NotificationType.Info,
            NotificationPriority.High,
            "Title",
            "Title Ar",
            "Message",
            "Message Ar",
            Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserId cannot be empty*");
    }

    [Fact]
    public void MarkAsRead_ShouldSetIsReadTrueAndReadAt_WhenNotAlreadyRead()
    {
        // Arrange
        var notification = CreateValidNotification();

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsRead_ShouldDoNothing_WhenAlreadyRead()
    {
        // Arrange
        var notification = CreateValidNotification();
        notification.MarkAsRead();
        var readAt = notification.ReadAt;

        // Act
        notification.MarkAsRead();

        // Assert
        notification.ReadAt.Should().Be(readAt);
    }

    [Fact]
    public void MarkAsUnread_ShouldSetIsReadFalseAndReadAtNull_WhenAlreadyRead()
    {
        // Arrange
        var notification = CreateValidNotification();
        notification.MarkAsRead();

        // Act
        notification.MarkAsUnread();

        // Assert
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsUnread_ShouldDoNothing_WhenAlreadyUnread()
    {
        // Arrange
        var notification = CreateValidNotification();

        // Act
        notification.MarkAsUnread();

        // Assert
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateStatusproperty()
    {
        // Arrange
        var notification = CreateValidNotification();
        var newStatus = NotificationStatus.Sent;

        // Act
        notification.UpdateStatus(newStatus);

        // Assert
        notification.Status.Should().Be(newStatus);
    }

    private Notification CreateValidNotification()
    {
        return new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "Title Ar",
            "Message",
            "Message Ar",
            Guid.NewGuid());
    }
}
