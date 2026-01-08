using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.UseCases;

public class UpdateNotificationStatusUseCaseTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateNotificationStatusUseCase _useCase;

    public UpdateNotificationStatusUseCaseTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _useCase = new UpdateNotificationStatusUseCase(
            _notificationManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateStatusToRead_WhenSuccessful()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var isRead = true;

        _notificationManagerMock.Setup(x => x.UpdateNotificationStatusAsync(notificationId, isRead, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(notificationId, isRead);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.UpdateNotificationStatusAsync(notificationId, isRead, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateStatusToUnread_WhenSuccessful()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var isRead = false;

        _notificationManagerMock.Setup(x => x.UpdateNotificationStatusAsync(notificationId, isRead, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(notificationId, isRead);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.UpdateNotificationStatusAsync(notificationId, isRead, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenManagerThrowsException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var isRead = true;

        _notificationManagerMock.Setup(x => x.UpdateNotificationStatusAsync(notificationId, isRead, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _useCase.ExecuteAsync(notificationId, isRead);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.UpdateFailed");
        result.Error.Message.Should().Be("Update failed");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSaveChanges_WhenOperationSucceeds()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.UpdateNotificationStatusAsync(notificationId, true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(notificationId, true);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotCallSaveChanges_WhenOperationFails()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.UpdateNotificationStatusAsync(notificationId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        await _useCase.ExecuteAsync(notificationId, true);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

