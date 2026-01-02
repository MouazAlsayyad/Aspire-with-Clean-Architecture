using AspireApp.Domain.Shared.Interfaces;
using System.Threading.Channels;

namespace AspireApp.ApiService.Infrastructure.Services;

/// <summary>
/// Implementation of IBackgroundTaskQueue using System.Threading.Channels for efficient task queuing.
/// Provides a thread-safe, unbounded queue for background tasks with cancellation support.
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>();

    public BackgroundTaskQueue()
    {
    }

    /// <inheritdoc />
    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem));

        _queue.Writer.TryWrite(workItem);
    }

    /// <inheritdoc />
    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}

