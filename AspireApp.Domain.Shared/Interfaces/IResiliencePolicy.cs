namespace AspireApp.Domain.Shared.Interfaces;

/// <summary>
/// Interface for executing operations with resilience policies (retry, circuit breaker, etc.)
/// </summary>
public interface IResiliencePolicy
{
    /// <summary>
    /// Executes an asynchronous operation with resilience policies applied
    /// </summary>
    /// <typeparam name="TResult">The return type of the operation</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with resilience policies applied
    /// </summary>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);
}

