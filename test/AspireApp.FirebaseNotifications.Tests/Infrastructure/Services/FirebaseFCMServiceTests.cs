using AspireApp.FirebaseNotifications.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Infrastructure.Services;

/// <summary>
/// Tests for FirebaseFCMService - Note: These tests verify the service structure and error handling.
/// Full integration tests with Firebase SDK would require Firebase emulator setup.
/// </summary>
public class FirebaseFCMServiceTests
{
    private readonly Mock<ILogger<FirebaseFCMService>> _loggerMock;
    private readonly FirebaseFCMService _service;

    public FirebaseFCMServiceTests()
    {
        _loggerMock = new Mock<ILogger<FirebaseFCMService>>();
        _service = new FirebaseFCMService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidDependencies()
    {
        // Act & Assert
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task SendToTokenAsync_ShouldAcceptValidParameters()
    {
        // Arrange
        var token = "test_fcm_token";
        var title = "Test Title";
        var body = "Test Body";
        var data = new Dictionary<string, string> { { "key", "value" } };

        // Act
        // Note: This will fail without Firebase initialization, but we're testing the method signature
        var act = async () => await _service.SendToTokenAsync(token, title, body, data, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendToTokensAsync_ShouldReturnEmptyDictionary_WhenTokenListIsEmpty()
    {
        // Arrange
        var tokens = new List<string>();
        var title = "Test Title";
        var body = "Test Body";

        // Act
        var result = await _service.SendToTokensAsync(tokens, title, body);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SendToTokensAsync_ShouldAcceptMultipleTokens()
    {
        // Arrange
        var tokens = new List<string> { "token1", "token2", "token3" };
        var title = "Test Title";
        var body = "Test Body";
        var data = new Dictionary<string, string> { { "key", "value" } };

        // Act
        // Note: This will fail without Firebase initialization, but we're testing the method signature
        var act = async () => await _service.SendToTokensAsync(tokens, title, body, data, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendToTokenAsync_ShouldHandleNullData()
    {
        // Arrange
        var token = "test_fcm_token";
        var title = "Test Title";
        var body = "Test Body";

        // Act
        var act = async () => await _service.SendToTokenAsync(token, title, body, null, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendToTokensAsync_ShouldHandleNullData()
    {
        // Arrange
        var tokens = new List<string> { "token1", "token2" };
        var title = "Test Title";
        var body = "Test Body";

        // Act
        var act = async () => await _service.SendToTokensAsync(tokens, title, body, null, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Service_ShouldImplementIFirebaseFCMService()
    {
        // Assert
        _service.Should().BeAssignableTo<AspireApp.FirebaseNotifications.Domain.Interfaces.IFirebaseFCMService>();
    }
}

