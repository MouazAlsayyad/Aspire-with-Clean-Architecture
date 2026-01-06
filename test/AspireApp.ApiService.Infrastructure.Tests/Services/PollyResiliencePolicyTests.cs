using AspireApp.ApiService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using System.Net;

namespace AspireApp.ApiService.Infrastructure.Tests.Services;

public class PollyResiliencePolicyTests
{
    private readonly ILogger<PollyResiliencePolicy> _logger;
    private readonly IConfiguration _configuration;
    private readonly PollyResiliencePolicy _sut;

    public PollyResiliencePolicyTests()
    {
        _logger = Substitute.For<ILogger<PollyResiliencePolicy>>();
        _configuration = Substitute.For<IConfiguration>();

        // Mock configuration values
        var maxRetrySection = Substitute.For<IConfigurationSection>();
        maxRetrySection.Value.Returns("2");
        _configuration.GetSection("Resilience:MaxRetryAttempts").Returns(maxRetrySection);

        var delaySection = Substitute.For<IConfigurationSection>();
        delaySection.Value.Returns("1");
        _configuration.GetSection("Resilience:DelaySeconds").Returns(delaySection);

        _sut = new PollyResiliencePolicy(_logger, _configuration);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResult_WhenOperationSucceeds()
    {
        // Arrange
        var expectedResult = "Success";
        Func<Task<string>> operation = () => Task.FromResult(expectedResult);

        // Act
        var result = await _sut.ExecuteAsync(operation);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRetry_WhenTransientExceptionOccurs()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            if (callCount == 1)
                throw new HttpRequestException("Transient error");
            return Task.FromResult("Success");
        };

        // Act
        var result = await _sut.ExecuteAsync(operation);

        // Assert
        result.Should().Be("Success");
        callCount.Should().Be(2);
        _logger.ReceivedWithAnyArgs(1).LogWarning(default!);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenMaxRetriesExceeded()
    {
        // Arrange
        int callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            throw new HttpRequestException("Persistent error");
        };

        // Act
        var act = async () => await _sut.ExecuteAsync(operation);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        callCount.Should().Be(3); // 1 initial + 2 retries
    }

    [Fact]
    public async Task ExecuteAsync_WithoutResult_ShouldRetry_WhenTransientExceptionOccurs()
    {
        // Arrange
        var callCount = 0;
        Func<Task> operation = () =>
        {
            callCount++;
            if (callCount == 1)
                throw new TimeoutException("Transient timeout");
            return Task.CompletedTask;
        };

        // Act
        await _sut.ExecuteAsync(operation);

        // Assert
        callCount.Should().Be(2);
    }
}
