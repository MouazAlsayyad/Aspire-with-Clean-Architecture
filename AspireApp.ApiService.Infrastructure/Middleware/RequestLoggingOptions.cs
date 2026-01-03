namespace AspireApp.ApiService.Infrastructure.Middleware;

/// <summary>
/// Configuration options for HTTP request logging middleware
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Threshold in milliseconds above which a request is considered slow and logged with warning level
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Maximum length of response body to log (prevents logging huge responses)
    /// </summary>
    public int MaxResponseBodyLength { get; set; } = 10000;

    /// <summary>
    /// Paths to exclude from logging (e.g., health checks, metrics)
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new();

    /// <summary>
    /// Whether to log request and response bodies (can impact performance)
    /// </summary>
    public bool LogRequestBody { get; set; } = false;

    /// <summary>
    /// Whether to log response bodies (can impact performance)
    /// </summary>
    public bool LogResponseBody { get; set; } = false;
}

