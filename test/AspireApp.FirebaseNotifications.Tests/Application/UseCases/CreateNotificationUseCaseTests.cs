
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AspireApp.FirebaseNotifications.Domain.Entities;
using AspireApp.FirebaseNotifications.Domain.Enums;
using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.UseCases;

public class CreateNotificationUseCaseTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateNotificationUseCase _useCase;

    public CreateNotificationUseCaseTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        // Setup BaseUseCase.ExecuteAndSaveAsync default behavior if it relies on UnitOfWork
        // Usually BaseUseCase calls uow.SaveChangesAsync() inside ExecuteAndSaveAsync wrapper.
        // Since ExecuteAndSaveAsync is protected in BaseUseCase but takes a delegate,
        // we assume the use case calls the delegate.
        // We might need to ensure UnitOfWork mocks are set up correctly if BaseUseCase logic is complex.
        // But for unit testing the logic inside the delegate, we just need to verify the manager call.
        
        _useCase = new CreateNotificationUseCase(
            _notificationManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallManagerAndReturnSuccess_WhenValid()
    {
        // Arrange
        var dto = new CreateNotificationDto(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            Guid.NewGuid(),
            "Url"
        );

        _notificationManagerMock.Setup(x => x.CreateNotificationAsync(
            dto.Type, dto.Priority, dto.Title, dto.TitleAr, dto.Message, dto.MessageAr, dto.UserId, dto.ActionUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notification(NotificationType.Info, NotificationPriority.Normal, "T", "Ta", "M", "Ma", Guid.NewGuid()));

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationManagerMock.Verify(x => x.CreateNotificationAsync(
            dto.Type, dto.Priority, dto.Title, dto.TitleAr, dto.Message, dto.MessageAr, dto.UserId, dto.ActionUrl, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify SaveChanges was called (implied by BaseUseCase usually)
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenManagerThrowsException()
    {
        // Arrange
        var dto = new CreateNotificationDto(
            NotificationType.Info,
            NotificationPriority.Normal,
            "Title",
            "TitleAr",
            "Message",
            "MessageAr",
            Guid.NewGuid(),
            "Url"
        );

        _notificationManagerMock.Setup(x => x.CreateNotificationAsync(
            It.IsAny<NotificationType>(), It.IsAny<NotificationPriority>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Some error"));

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.CreateFailed");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
