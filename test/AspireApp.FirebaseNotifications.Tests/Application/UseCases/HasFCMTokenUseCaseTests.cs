using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Application.UseCases;

public class HasFCMTokenUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly HasFCMTokenUseCase _useCase;

    public HasFCMTokenUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _useCase = new HasFCMTokenUseCase(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrue_WhenUserHasFCMToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("valid_fcm_token");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenUserHasNoFCMToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        // FCM token is null by default

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenUserHasEmptyFCMToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenUserHasWhitespaceFCMToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateFcmToken("   ");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenExceptionThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("General.ServerError");
    }
}

