using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Application.UseCases;

public class RegisterFCMTokenUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserManager> _userManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RegisterFCMTokenUseCase _useCase;

    public RegisterFCMTokenUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userManagerMock = new Mock<IUserManager>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _useCase = new RegisterFCMTokenUseCase(
            _userRepositoryMock.Object,
            _userManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallUserManagerAndReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new RegisterFCMTokenDto("new-token");

        // Create a mock User object
        var user = new User(
            "test@example.com",
            "testuser",
            PasswordHash.Create("hashedPassword", "salt"),
            "John",
            "Doe");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, default, default))
            .ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(userId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.UpdateFcmTokenAsync(user, dto.ClientFcmToken, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new RegisterFCMTokenDto("new-token");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, default, default))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        // DomainErrors.User.NotFound result check
        // We verify failure property.
        _userManagerMock.Verify(x => x.UpdateFcmTokenAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
