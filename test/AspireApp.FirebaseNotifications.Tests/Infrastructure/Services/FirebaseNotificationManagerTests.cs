using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.FirebaseNotifications.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Infrastructure.Services;

public class FirebaseNotificationManagerTests
{
    private readonly Mock<IFirebaseFCMService> _fcmServiceMock;
    private readonly Mock<ILogger<FirebaseNotificationManager>> _loggerMock;
    private readonly FirebaseNotificationManager _manager;

    public FirebaseNotificationManagerTests()
    {
        _fcmServiceMock = new Mock<IFirebaseFCMService>();
        _loggerMock = new Mock<ILogger<FirebaseNotificationManager>>();
        _manager = new FirebaseNotificationManager(_fcmServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldSendWithEnglishContent_WhenLanguageIsEnglish()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "English Title",
            "Arabic Title",
            "English Message",
            "Arabic Message",
            userId,
            "https://example.com/action");

        var fcmToken = "test_fcm_token";
        var language = "en";

        _fcmServiceMock.Setup(x => x.SendToTokenAsync(
            fcmToken,
            "English Title",
            "English Message",
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _manager.SendNotificationWithActionAsync(notification, fcmToken, language);

        // Assert
        result.Should().BeTrue();
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            fcmToken,
            "English Title",
            "English Message",
            It.Is<Dictionary<string, string>>(d =>
                d["notificationId"] == notification.Id.ToString() &&
                d["type"] == ((int)notification.Type).ToString() &&
                d["priority"] == ((int)notification.Priority).ToString() &&
                d["actionUrl"] == notification.ActionUrl),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldSendWithArabicContent_WhenLanguageIsArabic()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "English Title",
            "عنوان عربي",
            "English Message",
            "رسالة عربية",
            userId);

        var fcmToken = "test_fcm_token";
        var language = "ar";

        _fcmServiceMock.Setup(x => x.SendToTokenAsync(
            fcmToken,
            "عنوان عربي",
            "رسالة عربية",
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _manager.SendNotificationWithActionAsync(notification, fcmToken, language);

        // Assert
        result.Should().BeTrue();
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            fcmToken,
            "عنوان عربي",
            "رسالة عربية",
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldIncludeActionUrl_WhenProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var actionUrl = "https://example.com/action/123";
        var notification = new Notification(
            NotificationType.Warning,
            NotificationPriority.High,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId,
            actionUrl);

        var fcmToken = "test_fcm_token";

        _fcmServiceMock.Setup(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _manager.SendNotificationWithActionAsync(notification, fcmToken, "en");

        // Assert
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<Dictionary<string, string>>(d => d.ContainsKey("actionUrl") && d["actionUrl"] == actionUrl),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldNotIncludeActionUrl_WhenNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var fcmToken = "test_fcm_token";

        _fcmServiceMock.Setup(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _manager.SendNotificationWithActionAsync(notification, fcmToken, "en");

        // Assert
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<Dictionary<string, string>>(d => !d.ContainsKey("actionUrl")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldReturnFalse_WhenFcmTokenIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var fcmToken = "";

        // Act
        var result = await _manager.SendNotificationWithActionAsync(notification, fcmToken, "en");

        // Assert
        result.Should().BeFalse();
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldReturnFalse_WhenFcmTokenIsWhitespace()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var fcmToken = "   ";

        // Act
        var result = await _manager.SendNotificationWithActionAsync(notification, fcmToken, "en");

        // Assert
        result.Should().BeFalse();
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldThrowArgumentNullException_WhenNotificationIsNull()
    {
        // Arrange
        Notification? notification = null;
        var fcmToken = "test_token";

        // Act
        var act = async () => await _manager.SendNotificationWithActionAsync(notification!, fcmToken, "en");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldIncludeCustomData_WithNotificationId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Error,
            NotificationPriority.Critical,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var fcmToken = "test_fcm_token";

        _fcmServiceMock.Setup(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _manager.SendNotificationWithActionAsync(notification, fcmToken, "en");

        // Assert
        _fcmServiceMock.Verify(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<Dictionary<string, string>>(d =>
                d["notificationId"] == notification.Id.ToString() &&
                d["type"] == ((int)NotificationType.Error).ToString() &&
                d["priority"] == ((int)NotificationPriority.Critical).ToString()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithActionAsync_ShouldReturnFalse_WhenFcmServiceFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            userId);

        var fcmToken = "test_fcm_token";

        _fcmServiceMock.Setup(x => x.SendToTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _manager.SendNotificationWithActionAsync(notification, fcmToken, "en");

        // Assert
        result.Should().BeFalse();
    }
}

