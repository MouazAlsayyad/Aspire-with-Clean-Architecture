using AspireApp.FirebaseNotifications.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Infrastructure.Services;

public class NotificationLocalizationInitializerTests
{
    private readonly Mock<ILogger<NotificationLocalizationInitializer>> _loggerMock;
    private readonly NotificationLocalizationInitializer _initializer;

    public NotificationLocalizationInitializerTests()
    {
        _loggerMock = new Mock<ILogger<NotificationLocalizationInitializer>>();
        _initializer = new NotificationLocalizationInitializer(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidDependencies()
    {
        // Act & Assert
        _initializer.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidLogger()
    {
        // Act
        var act = () => new NotificationLocalizationInitializer(_loggerMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StartAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var act = async () => await _initializer.StartAsync(cancellationTokenSource.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var act = async () => await _initializer.StopAsync(cancellationTokenSource.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Service_ShouldInheritFromBackgroundService()
    {
        // Assert
        _initializer.Should().BeAssignableTo<Microsoft.Extensions.Hosting.BackgroundService>();
    }
}

