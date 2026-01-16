using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Domain.Events;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.FirebaseNotifications.Infrastructure.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Infrastructure.Handlers;

public class NotificationHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFirebaseNotificationManager> _firebaseNotificationManagerMock;
    private readonly Mock<ILogger<NotificationHandler>> _loggerMock;
    private readonly NotificationHandler _handler;

    public NotificationHandlerTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _firebaseNotificationManagerMock = new Mock<IFirebaseNotificationManager>();
        _loggerMock = new Mock<ILogger<NotificationHandler>>();

        _handler = new NotificationHandler(
            _notificationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _firebaseNotificationManagerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldSendNotification_WhenUserHasFcmToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("valid_fcm_token");
        user.UpdateLanguage("en");

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _firebaseNotificationManagerMock.Setup(x => x.SendNotificationWithActionAsync(
            notification, "valid_fcm_token", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _firebaseNotificationManagerMock.Verify(x => x.SendNotificationWithActionAsync(
            notification, "valid_fcm_token", "en", It.IsAny<CancellationToken>()), Times.Once);

        _notificationRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Sent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateStatusToSent_WhenSendingSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("valid_fcm_token");

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _firebaseNotificationManagerMock.Setup(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _notificationRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Sent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateStatusToFailed_WhenSendingFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("valid_fcm_token");

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _firebaseNotificationManagerMock.Setup(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _notificationRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendNotification_WhenNotificationNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _firebaseNotificationManagerMock.Verify(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateStatusToFailed_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _firebaseNotificationManagerMock.Verify(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _notificationRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendPushNotification_WhenUserHasNoFcmToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        // No FCM token set

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _firebaseNotificationManagerMock.Verify(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Notification should not be marked as failed when user has no FCM token
        _notificationRepositoryMock.Verify(x => x.UpdateAsync(
            It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseUserLanguage_WhenAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("valid_fcm_token");
        user.UpdateLanguage("ar");

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _firebaseNotificationManagerMock.Setup(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), "ar", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _firebaseNotificationManagerMock.Verify(x => x.SendNotificationWithActionAsync(
            notification, "valid_fcm_token", "ar", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldDefaultToEnglish_WhenUserLanguageIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("valid_fcm_token");
        // Language is null by default

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _firebaseNotificationManagerMock.Setup(x => x.SendNotificationWithActionAsync(
            It.IsAny<Notification>(), It.IsAny<string>(), "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _firebaseNotificationManagerMock.Verify(x => x.SendNotificationWithActionAsync(
            notification, "valid_fcm_token", "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateStatusToFailed_WhenExceptionThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        _notificationRepositoryMock.Setup(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _notificationRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleException_WhenUpdatingStatusFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var domainEvent = new NotificationCreatedEvent(notificationId, userId);

        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        _notificationRepositoryMock.SetupSequence(x => x.GetAsync(notificationId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification)
            .ReturnsAsync(notification);

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        _notificationRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

