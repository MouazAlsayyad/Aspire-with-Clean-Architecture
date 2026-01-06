using AspireApp.Domain.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Mail;

namespace AspireApp.ApiService.Infrastructure.Services;

/// <summary>
/// Implementation of IResiliencePolicy using Polly library
/// Provides retry policies with exponential backoff for transient failures
/// </summary>
public class PollyResiliencePolicy : IResiliencePolicy
{
    private readonly ILogger<PollyResiliencePolicy> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public PollyResiliencePolicy(ILogger<PollyResiliencePolicy> logger, IConfiguration configuration)
    {
        _logger = logger;

        // Read settings from configuration with defaults
        var maxRetryAttempts = configuration.GetValue<int>("Resilience:MaxRetryAttempts", 3);
        var delaySeconds = configuration.GetValue<int>("Resilience:DelaySeconds", 2);

        // Build resilience pipeline with retry strategy
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                // Maximum number of retry attempts
                MaxRetryAttempts = maxRetryAttempts,

                // Exponential backoff
                Delay = TimeSpan.FromSeconds(delaySeconds),
                BackoffType = DelayBackoffType.Exponential,

                // Add jitter to prevent thundering herd
                UseJitter = true,

                // Handle specific exceptions that are transient
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<SmtpException>(ex => IsTransientSmtpException(ex))
                    .Handle<WebException>(ex => IsTransientWebException(ex))
                    .Handle<TaskCanceledException>()
                    .Handle<OperationCanceledException>(),

                // Log retry attempts
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry attempt {AttemptNumber} after {Delay}ms due to: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown error");
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Executes an asynchronous operation with resilience policies applied
    /// </summary>
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _resiliencePipeline.ExecuteAsync(
                async _ => await operation(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed after all retry attempts");
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous operation with resilience policies applied
    /// </summary>
    public async Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _resiliencePipeline.ExecuteAsync(
                async _ => await operation(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed after all retry attempts");
            throw;
        }
    }

    /// <summary>
    /// Determines if an SMTP exception is transient and should be retried
    /// </summary>
    private static bool IsTransientSmtpException(SmtpException ex)
    {
        // Retry on temporary server errors
        return ex.StatusCode switch
        {
            SmtpStatusCode.ServiceNotAvailable => true,
            SmtpStatusCode.MailboxBusy => true,
            SmtpStatusCode.TransactionFailed => true,
            SmtpStatusCode.GeneralFailure => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if a WebException is transient and should be retried
    /// </summary>
    private static bool IsTransientWebException(WebException ex)
    {
        // Retry on network errors and timeouts
        return ex.Status switch
        {
            WebExceptionStatus.ConnectFailure => true,
            WebExceptionStatus.NameResolutionFailure => true,
            WebExceptionStatus.Timeout => true,
            WebExceptionStatus.ReceiveFailure => true,
            WebExceptionStatus.SendFailure => true,
            WebExceptionStatus.PipelineFailure => true,
            WebExceptionStatus.ConnectionClosed => true,
            WebExceptionStatus.KeepAliveFailure => true,
            _ => false
        };
    }
}

