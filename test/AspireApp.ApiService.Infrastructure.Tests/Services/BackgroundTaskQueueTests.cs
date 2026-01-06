using AspireApp.ApiService.Infrastructure.Services;
using FluentAssertions;

namespace AspireApp.ApiService.Infrastructure.Tests.Services;

public class BackgroundTaskQueueTests
{
    private readonly BackgroundTaskQueue _sut;

    public BackgroundTaskQueueTests()
    {
        _sut = new BackgroundTaskQueue();
    }

    [Fact]
    public async Task QueueBackgroundWorkItem_ShouldQueueAndDequeue_WhenWorkItemIsProvided()
    {
        // Arrange
        var callCount = 0;
        Func<CancellationToken, Task> workItem = (ct) =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        // Act
        _sut.QueueBackgroundWorkItem(workItem);
        var dequeuedItem = await _sut.DequeueAsync(CancellationToken.None);
        await dequeuedItem(CancellationToken.None);

        // Assert
        dequeuedItem.Should().Be(workItem);
        callCount.Should().Be(1);
    }

    [Fact]
    public void QueueBackgroundWorkItem_ShouldThrowArgumentNullException_WhenWorkItemIsNull()
    {
        // Act
        var act = () => _sut.QueueBackgroundWorkItem(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task DequeueAsync_ShouldBeCancelled_WhenCancellationTokenIsCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _sut.DequeueAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
