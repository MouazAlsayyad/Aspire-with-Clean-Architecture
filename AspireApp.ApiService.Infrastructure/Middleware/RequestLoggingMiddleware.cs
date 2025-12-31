using AspireApp.ApiService.Domain.ActivityLogs.Enums;
using AspireApp.ApiService.Domain.ActivityLogs.Interfaces;
using AspireApp.ApiService.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AspireApp.ApiService.Infrastructure.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with timing information
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        RequestLoggingOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RequestLoggingOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for certain paths (health checks, etc.)
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Resolve scoped service from request scope
        var activityLogger = context.RequestServices.GetRequiredService<IActivityLogger>();

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Capture request details
        var requestDetails = await CaptureRequestAsync(context, requestId);

        // Capture original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a memory stream to capture response
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var durationMs = stopwatch.ElapsedMilliseconds;

            // Capture response details
            var responseDetails = await CaptureResponseAsync(context, requestId);

            // Restore original body stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            // Log the request/response
            await LogRequestResponseAsync(
                context,
                activityLogger,
                requestId,
                requestDetails,
                responseDetails,
                durationMs,
                exception);
        }
    }

    private bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? "";

        // Skip health checks and other system endpoints
        return _options.ExcludedPaths.Any(excluded =>
            pathValue.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<RequestDetails> CaptureRequestAsync(HttpContext context, string requestId)
    {
        var request = context.Request;

        // Capture headers (filtered)
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }
        var filteredHeaders = SensitiveDataFilter.FilterHeaders(headers);

        // Capture query string (filtered)
        var queryString = request.QueryString.ToString();
        var filteredQueryString = SensitiveDataFilter.FilterString(queryString);

        // Capture request body
        string? requestBody = null;
        if (request.ContentLength > 0 && request.ContentType != null)
        {
            // Enable buffering to allow reading the body multiple times
            request.EnableBuffering();

            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin); // Reset for next middleware

            if (!string.IsNullOrWhiteSpace(body))
            {
                requestBody = SensitiveDataFilter.FilterString(body);

                // Try to format JSON if it's JSON content
                if (request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        requestBody = SensitiveDataFilter.FilterJson(body);
                    }
                    catch
                    {
                        // If JSON parsing fails, use the filtered string
                    }
                }
            }
        }

        return new RequestDetails
        {
            RequestId = requestId,
            Method = request.Method,
            Path = request.Path.Value ?? "",
            QueryString = filteredQueryString,
            Headers = filteredHeaders,
            Body = requestBody,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength ?? 0,
            Scheme = request.Scheme,
            Host = request.Host.Value ?? "",
            Protocol = request.Protocol
        };
    }

    private async Task<ResponseDetails> CaptureResponseAsync(HttpContext context, string requestId)
    {
        var response = context.Response;

        // Capture headers (filtered)
        var headers = new Dictionary<string, string>();
        foreach (var header in response.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }
        var filteredHeaders = SensitiveDataFilter.FilterHeaders(headers);

        // Capture response body
        string? responseBody = null;
        if (response.Body is MemoryStream memoryStream && memoryStream.Length > 0)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            memoryStream.Seek(0, SeekOrigin.Begin); // Reset for copying

            if (!string.IsNullOrWhiteSpace(body) && body.Length <= _options.MaxResponseBodyLength)
            {
                responseBody = SensitiveDataFilter.FilterString(body);

                // Try to format JSON if it's JSON content
                if (response.ContentType != null &&
                    response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        responseBody = SensitiveDataFilter.FilterJson(body);
                    }
                    catch
                    {
                        // If JSON parsing fails, use the filtered string
                    }
                }
            }
            else if (body.Length > _options.MaxResponseBodyLength)
            {
                responseBody = $"[Response body too large ({body.Length} bytes), truncated]";
            }
        }

        return new ResponseDetails
        {
            RequestId = requestId,
            StatusCode = response.StatusCode,
            Headers = filteredHeaders,
            Body = responseBody,
            ContentType = response.ContentType ?? "",
            ContentLength = response.ContentLength ?? 0
        };
    }

    private async Task LogRequestResponseAsync(
        HttpContext context,
        IActivityLogger activityLogger,
        string requestId,
        RequestDetails requestDetails,
        ResponseDetails responseDetails,
        long durationMs,
        Exception? exception)
    {
        try
        {
            // Determine activity type based on status code and duration
            var activityType = DetermineActivityType(responseDetails.StatusCode, durationMs);
            var severity = DetermineSeverity(responseDetails.StatusCode, durationMs);

            // Build metadata
            var metadata = new Dictionary<string, object>
            {
                ["RequestId"] = requestId,
                ["Method"] = requestDetails.Method,
                ["Path"] = requestDetails.Path,
                ["QueryString"] = requestDetails.QueryString ?? "",
                ["StatusCode"] = responseDetails.StatusCode,
                ["DurationMs"] = durationMs,
                ["RequestHeaders"] = requestDetails.Headers,
                ["ResponseHeaders"] = responseDetails.Headers,
                ["RequestContentType"] = requestDetails.ContentType ?? "",
                ["ResponseContentType"] = responseDetails.ContentType ?? "",
                ["RequestContentLength"] = requestDetails.ContentLength,
                ["ResponseContentLength"] = responseDetails.ContentLength ?? 0
            };

            if (!string.IsNullOrWhiteSpace(requestDetails.Body))
            {
                metadata["RequestBody"] = requestDetails.Body;
            }

            if (!string.IsNullOrWhiteSpace(responseDetails.Body))
            {
                metadata["ResponseBody"] = responseDetails.Body;
            }

            if (exception != null)
            {
                metadata["Exception"] = exception.Message;
                metadata["ExceptionType"] = exception.GetType().Name;
            }

            // Build description
            var description = $"{requestDetails.Method} {requestDetails.Path} - {responseDetails.StatusCode} ({durationMs}ms)";
            if (!string.IsNullOrWhiteSpace(requestDetails.QueryString))
            {
                description += $"?{requestDetails.QueryString}";
            }

            // Log to activity log system
            await activityLogger.LogAsync(
                activityType,
                description,
                entityId: null,
                entityType: null,
                metadata: metadata,
                severity: severity,
                isPublic: false, // HTTP logs are internal
                "http", "request", "api");

            // Log to Serilog
            var logLevel = GetLogLevel(responseDetails.StatusCode, durationMs, exception);
            var logMessage = $"[{requestId}] {requestDetails.Method} {requestDetails.Path} - {responseDetails.StatusCode} ({durationMs}ms)";

            if (exception != null)
            {
                _logger.Log(logLevel, exception, logMessage);
            }
            else
            {
                _logger.Log(logLevel, logMessage);
            }
        }
        catch (Exception ex)
        {
            // Don't let logging errors break the request
            _logger.LogError(ex, "Failed to log HTTP request/response");
        }
    }

    private string DetermineActivityType(int statusCode, long durationMs)
    {
        if (statusCode >= 500)
        {
            return "HttpRequestError";
        }

        if (durationMs > _options.SlowRequestThresholdMs)
        {
            return "HttpRequestSlow";
        }

        return "HttpRequest";
    }

    private ActivitySeverity DetermineSeverity(int statusCode, long durationMs)
    {
        if (statusCode >= 500)
        {
            return ActivitySeverity.Critical;
        }

        if (statusCode >= 400)
        {
            return ActivitySeverity.Medium;
        }

        if (durationMs > _options.SlowRequestThresholdMs)
        {
            return ActivitySeverity.Low;
        }

        return ActivitySeverity.Info;
    }

    private Microsoft.Extensions.Logging.LogLevel GetLogLevel(int statusCode, long durationMs, Exception? exception)
    {
        if (exception != null || statusCode >= 500)
        {
            return Microsoft.Extensions.Logging.LogLevel.Error;
        }

        if (statusCode >= 400)
        {
            return Microsoft.Extensions.Logging.LogLevel.Warning;
        }

        if (durationMs > _options.SlowRequestThresholdMs)
        {
            return Microsoft.Extensions.Logging.LogLevel.Information;
        }

        return Microsoft.Extensions.Logging.LogLevel.Information;
    }

    private class RequestDetails
    {
        public string RequestId { get; set; } = "";
        public string Method { get; set; } = "";
        public string Path { get; set; } = "";
        public string? QueryString { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? Body { get; set; }
        public string? ContentType { get; set; }
        public long ContentLength { get; set; }
        public string Scheme { get; set; } = "";
        public string Host { get; set; } = "";
        public string Protocol { get; set; } = "";
    }

    private class ResponseDetails
    {
        public string RequestId { get; set; } = "";
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? Body { get; set; }
        public string ContentType { get; set; } = "";
        public long? ContentLength { get; set; }
    }
}

/// <summary>
/// Configuration options for request logging middleware
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Paths to exclude from logging (e.g., health checks)
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/alive",
        "/metrics"
    };

    /// <summary>
    /// Threshold in milliseconds for considering a request as slow
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Maximum response body length to log (in characters)
    /// </summary>
    public int MaxResponseBodyLength { get; set; } = 10000;
}

