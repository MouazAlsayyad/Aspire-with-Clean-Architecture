using AspireApp.ApiService.Infrastructure.Services;
using AspireApp.Domain.Shared.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AspireApp.ApiService.Infrastructure.Tests.Services;

public class QueuedHostedServiceTests
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly QueuedHostedService _sut;

    public QueuedHostedServiceTests()
    {
        _taskQueue = Substitute.For<IBackgroundTaskQueue>();
        _logger = Substitute.For<ILogger<QueuedHostedService>>();
        _sut = new QueuedHostedService(_taskQueue, _logger);
    }

    [Fact]
    public async Task StartAsync_ShouldProcessQueuedItem()
    {
        // Arrange
        var processed = false;
        Func<CancellationToken, Task> workItem = token =>
        {
            processed = true;
            return Task.CompletedTask;
        };

        using var cts = new CancellationTokenSource();

        // Setup mock to return work item then cancel
        _taskQueue.DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(
                new ValueTask<Func<CancellationToken, Task>>(workItem), // First call
                new ValueTask<Func<CancellationToken, Task>>(async token => // Second call
                {
                    await Task.Delay(10, token); // Small delay
                    cts.Cancel(); // Cancel loop
                    throw new OperationCanceledException();
                })
            );

        // Act
        var executeTask = _sut.StartAsync(cts.Token);

        // Wait for loop to potentially run
        await Task.Delay(200);
        cts.Cancel(); // Ensure cancellation if mock didn't trigger

        try
        {
            await executeTask;
        }
        catch (OperationCanceledException) { }

        await _sut.StopAsync(CancellationToken.None);

        // Assert
        processed.Should().BeTrue();
    }
}
