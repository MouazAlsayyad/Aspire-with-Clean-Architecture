using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.UseCases;

public class MarkAllNotificationsAsReadUseCaseTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MarkAllNotificationsAsReadUseCase _useCase;

    public MarkAllNotificationsAsReadUseCaseTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _useCase = new MarkAllNotificationsAsReadUseCase(
            _notificationManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkAllAsReadAndReturnCount_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _notificationManagerMock.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedCount);
        _notificationManagerMock.Verify(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoNotificationsToMark()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _notificationManagerMock.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        _notificationManagerMock.Verify(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenManagerThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _notificationManagerMock.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.MarkAllReadFailed");
        result.Error.Message.Should().Be("Database error");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSaveChanges_WhenOperationSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        await _useCase.ExecuteAsync(userId);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

