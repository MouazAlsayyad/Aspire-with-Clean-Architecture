using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.Mappings;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.Mappings;

public class NotificationMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public NotificationMappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<NotificationMappingProfile>();
        });
        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_ShouldBeValid()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_ShouldMapNotificationToNotificationDto_WithAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Test Title",
            "عنوان الاختبار",
            "Test Message",
            "رسالة الاختبار",
            userId,
            "https://example.com/action");

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(notification.Id);
        dto.Type.Should().Be(NotificationType.Info);
        dto.Priority.Should().Be(NotificationPriority.Normal);
        dto.Status.Should().Be(notification.Status);
        dto.IsRead.Should().Be(notification.IsRead);
        dto.ReadAt.Should().Be(notification.ReadAt);
        dto.Title.Should().Be("Test Title");
        dto.TitleAr.Should().Be("عنوان الاختبار");
        dto.Message.Should().Be("Test Message");
        dto.MessageAr.Should().Be("رسالة الاختبار");
        dto.ActionUrl.Should().Be("https://example.com/action");
        dto.UserId.Should().Be(userId);
        dto.CreationTime.Should().Be(notification.CreationTime);
    }

    [Fact]
    public void Map_ShouldMapNotificationWithoutActionUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(
            NotificationType.Warning,
            NotificationPriority.High,
            "Warning Title",
            "عنوان التحذير",
            "Warning Message",
            "رسالة التحذير",
            userId);

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.Should().NotBeNull();
        dto.ActionUrl.Should().BeNull();
        dto.Title.Should().Be("Warning Title");
        dto.Message.Should().Be("Warning Message");
    }

    [Fact]
    public void Map_ShouldPreserveNotificationType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var infoNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var warningNotification = new Notification(NotificationType.Warning, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var errorNotification = new Notification(NotificationType.Error, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var successNotification = new Notification(NotificationType.Success, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);

        // Act
        var infoDto = _mapper.Map<NotificationDto>(infoNotification);
        var warningDto = _mapper.Map<NotificationDto>(warningNotification);
        var errorDto = _mapper.Map<NotificationDto>(errorNotification);
        var successDto = _mapper.Map<NotificationDto>(successNotification);

        // Assert
        infoDto.Type.Should().Be(NotificationType.Info);
        warningDto.Type.Should().Be(NotificationType.Warning);
        errorDto.Type.Should().Be(NotificationType.Error);
        successDto.Type.Should().Be(NotificationType.Success);
    }

    [Fact]
    public void Map_ShouldPreserveNotificationPriority()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var lowNotification = new Notification(NotificationType.Info, NotificationPriority.Low, "T", "TA", "M", "MA", userId);
        var normalNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var highNotification = new Notification(NotificationType.Info, NotificationPriority.High, "T", "TA", "M", "MA", userId);
        var criticalNotification = new Notification(NotificationType.Info, NotificationPriority.Critical, "T", "TA", "M", "MA", userId);

        // Act
        var lowDto = _mapper.Map<NotificationDto>(lowNotification);
        var normalDto = _mapper.Map<NotificationDto>(normalNotification);
        var highDto = _mapper.Map<NotificationDto>(highNotification);
        var criticalDto = _mapper.Map<NotificationDto>(criticalNotification);

        // Assert
        lowDto.Priority.Should().Be(NotificationPriority.Low);
        normalDto.Priority.Should().Be(NotificationPriority.Normal);
        highDto.Priority.Should().Be(NotificationPriority.High);
        criticalDto.Priority.Should().Be(NotificationPriority.Critical);
    }

    [Fact]
    public void Map_ShouldPreserveIsReadStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unreadNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var readNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        readNotification.MarkAsRead();

        // Act
        var unreadDto = _mapper.Map<NotificationDto>(unreadNotification);
        var readDto = _mapper.Map<NotificationDto>(readNotification);

        // Assert
        unreadDto.IsRead.Should().BeFalse();
        unreadDto.ReadAt.Should().BeNull();
        readDto.IsRead.Should().BeTrue();
        readDto.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public void Map_ShouldMapListOfNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            new Notification(NotificationType.Info, NotificationPriority.Normal, "Title1", "TitleAr1", "Message1", "MessageAr1", userId),
            new Notification(NotificationType.Warning, NotificationPriority.High, "Title2", "TitleAr2", "Message2", "MessageAr2", userId),
            new Notification(NotificationType.Error, NotificationPriority.Critical, "Title3", "TitleAr3", "Message3", "MessageAr3", userId)
        };

        // Act
        var dtos = _mapper.Map<List<NotificationDto>>(notifications);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().HaveCount(3);
        dtos[0].Title.Should().Be("Title1");
        dtos[1].Title.Should().Be("Title2");
        dtos[2].Title.Should().Be("Title3");
    }

    [Fact]
    public void Map_ShouldPreserveNotificationStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pendingNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var sentNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        sentNotification.UpdateStatus(NotificationStatus.Sent);
        var failedNotification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        failedNotification.UpdateStatus(NotificationStatus.Failed);

        // Act
        var pendingDto = _mapper.Map<NotificationDto>(pendingNotification);
        var sentDto = _mapper.Map<NotificationDto>(sentNotification);
        var failedDto = _mapper.Map<NotificationDto>(failedNotification);

        // Assert
        pendingDto.Status.Should().Be(NotificationStatus.Pending);
        sentDto.Status.Should().Be(NotificationStatus.Sent);
        failedDto.Status.Should().Be(NotificationStatus.Failed);
    }

    [Fact]
    public void Map_ShouldPreserveCreationTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var expectedCreationTime = notification.CreationTime;

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.CreationTime.Should().Be(expectedCreationTime);
    }

    [Fact]
    public void Map_ShouldHandleMinimalValidStrings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.Should().NotBeNull();
        dto.Title.Should().Be("T");
        dto.TitleAr.Should().Be("TA");
        dto.Message.Should().Be("M");
        dto.MessageAr.Should().Be("MA");
    }

    [Fact]
    public void Map_ShouldPreserveUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.UserId.Should().Be(userId);
    }

    [Fact]
    public void Map_ShouldPreserveNotificationId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "TA", "M", "MA", userId);
        var expectedId = notification.Id;

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.Id.Should().Be(expectedId);
        dto.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Map_ShouldHandleLongStrings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var longTitle = new string('A', 1000);
        var longMessage = new string('B', 5000);
        var notification = new Notification(
            NotificationType.Info, 
            NotificationPriority.Normal, 
            longTitle, 
            "TitleAr", 
            longMessage, 
            "MessageAr", 
            userId);

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.Title.Should().Be(longTitle);
        dto.Message.Should().Be(longMessage);
    }

    [Fact]
    public void Map_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var specialTitle = "Title with <html> & special \"characters\"";
        var specialMessage = "Message with \n newlines \t tabs and 😀 emojis";
        var notification = new Notification(
            NotificationType.Info, 
            NotificationPriority.Normal, 
            specialTitle, 
            "TitleAr", 
            specialMessage, 
            "MessageAr", 
            userId);

        // Act
        var dto = _mapper.Map<NotificationDto>(notification);

        // Assert
        dto.Title.Should().Be(specialTitle);
        dto.Message.Should().Be(specialMessage);
    }
}

