using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Application.UseCases;

public class GetNotificationsUseCaseTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetNotificationsUseCase _useCase;

    public GetNotificationsUseCaseTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _useCase = new GetNotificationsUseCase(
            _notificationManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPaginatedNotifications_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto(PageSize: 20, LastNotificationId: null, TimeFilter: NotificationTimeFilter.All);

        var notification1 = new Notification(NotificationType.Info, NotificationPriority.Normal, "Title1", "TitleAr1", "Message1", "MessageAr1", userId);
        var notification2 = new Notification(NotificationType.Warning, NotificationPriority.High, "Title2", "TitleAr2", "Message2", "MessageAr2", userId);
        var notifications = new List<Notification> { notification1, notification2 };

        var notificationDto1 = new NotificationDto(notification1.Id, notification1.Type, notification1.Priority, notification1.Status,
            notification1.IsRead, notification1.ReadAt, notification1.Title, notification1.TitleAr,
            notification1.Message, notification1.MessageAr, notification1.ActionUrl, notification1.UserId, notification1.CreationTime);
        var notificationDto2 = new NotificationDto(notification2.Id, notification2.Type, notification2.Priority, notification2.Status,
            notification2.IsRead, notification2.ReadAt, notification2.Title, notification2.TitleAr,
            notification2.Message, notification2.MessageAr, notification2.ActionUrl, notification2.UserId, notification2.CreationTime);
        var notificationDtos = new List<NotificationDto> { notificationDto1, notificationDto2 };

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, request.TimeFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((notifications, true));

        _mapperMock.Setup(x => x.Map<List<NotificationDto>>(notifications))
            .Returns(notificationDtos);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Notifications.Should().HaveCount(2);
        result.Value.HasMore.Should().BeTrue();
        result.Value.LastNotificationId.Should().Be(notification2.Id);

        _notificationManagerMock.Verify(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, request.TimeFilter, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto();

        var emptyNotifications = new List<Notification>();
        var emptyDtos = new List<NotificationDto>();

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, request.TimeFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyNotifications, false));

        _mapperMock.Setup(x => x.Map<List<NotificationDto>>(emptyNotifications))
            .Returns(emptyDtos);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Notifications.Should().BeEmpty();
        result.Value.HasMore.Should().BeFalse();
        result.Value.LastNotificationId.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFilterByTimeToday_WhenTodayFilterSpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto(TimeFilter: NotificationTimeFilter.Today);

        var notifications = new List<Notification>();
        var notificationDtos = new List<NotificationDto>();

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, NotificationTimeFilter.Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync((notifications, false));

        _mapperMock.Setup(x => x.Map<List<NotificationDto>>(notifications))
            .Returns(notificationDtos);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, NotificationTimeFilter.Today, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFilterByTimeYesterday_WhenYesterdayFilterSpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto(TimeFilter: NotificationTimeFilter.Yesterday);

        var notifications = new List<Notification>();
        var notificationDtos = new List<NotificationDto>();

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, NotificationTimeFilter.Yesterday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((notifications, false));

        _mapperMock.Setup(x => x.Map<List<NotificationDto>>(notifications))
            .Returns(notificationDtos);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, NotificationTimeFilter.Yesterday, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFilterByTimeEarlier_WhenEarlierFilterSpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto(TimeFilter: NotificationTimeFilter.Earlier);

        var notifications = new List<Notification>();
        var notificationDtos = new List<NotificationDto>();

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, NotificationTimeFilter.Earlier, It.IsAny<CancellationToken>()))
            .ReturnsAsync((notifications, false));

        _mapperMock.Setup(x => x.Map<List<NotificationDto>>(notifications))
            .Returns(notificationDtos);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.GetNotificationsAsync(
            userId, request.LastNotificationId, request.PageSize, NotificationTimeFilter.Earlier, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseCursorPagination_WhenLastNotificationIdProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lastNotificationId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto(LastNotificationId: lastNotificationId);

        var notifications = new List<Notification>();
        var notificationDtos = new List<NotificationDto>();

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, lastNotificationId, request.PageSize, request.TimeFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((notifications, false));

        _mapperMock.Setup(x => x.Map<List<NotificationDto>>(notifications))
            .Returns(notificationDtos);

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.GetNotificationsAsync(
            userId, lastNotificationId, request.PageSize, request.TimeFilter, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenExceptionThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetNotificationsRequestDto();

        _notificationManagerMock.Setup(x => x.GetNotificationsAsync(
            userId, It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<NotificationTimeFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(userId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.GetFailed");
        result.Error.Message.Should().Be("Database error");
    }
}

