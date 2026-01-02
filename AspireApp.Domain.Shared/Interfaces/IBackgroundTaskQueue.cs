namespace AspireApp.Domain.Shared.Interfaces;

/// <summary>
/// Interface for queuing background tasks to be executed in a controlled and cancellable way.
/// This provides a structured, scalable, and production-friendly approach to background task processing
/// compared to Task.Run, with support for graceful shutdown and lifecycle management.
/// 
/// <para>
/// <b>Why use IBackgroundTaskQueue instead of Task.Run?</b>
/// </para>
/// <list type="bullet">
/// <item>✅ Graceful shutdown via CancellationToken</item>
/// <item>✅ Centralized task queue management</item>
/// <item>✅ Respects app lifecycle (no orphaned tasks)</item>
/// <item>✅ Easier debugging and logging</item>
/// <item>✅ Better for scaling and reliability</item>
/// </list>
/// 
/// <para>
/// <b>Usage Example:</b>
/// </para>
/// <code>
/// // In a controller or service, inject IBackgroundTaskQueue:
/// public class MyController : ControllerBase
/// {
///     private readonly IBackgroundTaskQueue _backgroundTaskQueue;
///     
///     public MyController(IBackgroundTaskQueue backgroundTaskQueue)
///     {
///         _backgroundTaskQueue = backgroundTaskQueue;
///     }
///     
///     [HttpPost]
///     public IActionResult ProcessData()
///     {
///         // Queue a background task instead of using Task.Run
///         _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
///         {
///             // Your fire-and-forget logic here
///             await DoLongRunningWork(token);
///         });
///         
///         return Ok("Task queued successfully");
///     }
/// }
/// </code>
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Queues a background work item to be executed asynchronously.
    /// </summary>
    /// <param name="workItem">The work item to queue, which receives a CancellationToken for cancellation support.</param>
    /// <example>
    /// <code>
    /// _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
    /// {
    ///     // Your background work here
    ///     await ProcessDataAsync(token);
    /// });
    /// </code>
    /// </example>
    void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

    /// <summary>
    /// Dequeues a background work item asynchronously.
    /// This method is typically called by the QueuedHostedService to process queued tasks.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the dequeue operation.</param>
    /// <returns>A ValueTask containing the work item function.</returns>
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}

