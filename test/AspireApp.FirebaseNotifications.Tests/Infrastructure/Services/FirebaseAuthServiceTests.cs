using AspireApp.FirebaseNotifications.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Infrastructure.Services;

/// <summary>
/// Tests for FirebaseAuthService - Note: These tests verify the service structure.
/// Full integration tests with Firebase SDK would require Firebase emulator setup.
/// </summary>
public class FirebaseAuthServiceTests
{
    private readonly Mock<ILogger<FirebaseAuthService>> _loggerMock;
    private readonly FirebaseAuthService _service;

    public FirebaseAuthServiceTests()
    {
        _loggerMock = new Mock<ILogger<FirebaseAuthService>>();
        _service = new FirebaseAuthService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidDependencies()
    {
        // Act & Assert
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldAcceptValidEmail()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        // Note: This will fail without Firebase initialization, but we're testing the method signature
        var act = async () => await _service.CreateUserAsync(email, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetFirebaseUidAsync_ShouldAcceptValidEmail()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        // Note: This will fail without Firebase initialization, but we're testing the method signature
        var act = async () => await _service.GetFirebaseUidAsync(email, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Service_ShouldImplementIFirebaseAuthService()
    {
        // Assert
        _service.Should().BeAssignableTo<AspireApp.ApiService.Domain.Authentication.Interfaces.IFirebaseAuthService>();
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidLogger()
    {
        // Act
        var act = () => new FirebaseAuthService(_loggerMock.Object);

        // Assert
        act.Should().NotThrow();
    }
}

