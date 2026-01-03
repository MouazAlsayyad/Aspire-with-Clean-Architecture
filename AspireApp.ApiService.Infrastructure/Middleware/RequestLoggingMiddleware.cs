using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AspireApp.ApiService.Infrastructure.Middleware;

/// <summary>
/// Lightweight middleware for logging HTTP requests and responses using Serilog.
/// Optimized for performance - does not buffer request/response bodies by default.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        RequestLoggingOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for excluded paths
        if (_options.ExcludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var requestQuery = context.Request.QueryString.ToString();

        string? requestBody = null;
        if (_options.LogRequestBody && context.Request.ContentLength > 0)
        {
            requestBody = await ReadRequestBodyAsync(context.Request);
        }

        // Store original response body stream
        var originalResponseBodyStream = context.Response.Body;

        try
        {
            string? responseBody = null;

            // Only buffer response if we need to log it
            if (_options.LogResponseBody)
            {
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);

                stopwatch.Stop();

                // Read response body
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                responseBody = await ReadResponseBodyAsync(responseBodyStream);

                // Copy response back to original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            else
            {
                await _next(context);
                stopwatch.Stop();
            }

            // Log request completion
            LogRequest(
                requestMethod,
                requestPath,
                requestQuery,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestBody,
                responseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request {Method} {Path}{Query} failed after {ElapsedMs}ms",
                requestMethod,
                requestPath,
                requestQuery,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBodyStream;
        }
    }

    private void LogRequest(
        string method,
        PathString path,
        string query,
        int statusCode,
        long elapsedMs,
        string? requestBody,
        string? responseBody)
    {
        var isSlow = elapsedMs >= _options.SlowRequestThresholdMs;
        var logLevel = isSlow ? LogLevel.Warning : LogLevel.Information;

        if (_options.LogRequestBody || _options.LogResponseBody)
        {
            _logger.Log(logLevel,
                "HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs}ms. Request: {RequestBody}, Response: {ResponseBody}",
                method,
                path,
                query,
                statusCode,
                elapsedMs,
                requestBody ?? "N/A",
                responseBody ?? "N/A");
        }
        else
        {
            _logger.Log(logLevel,
                "HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs}ms",
                method,
                path,
                query,
                statusCode,
                elapsedMs);
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body.Length > _options.MaxResponseBodyLength
            ? body.Substring(0, _options.MaxResponseBodyLength) + "... (truncated)"
            : body;
    }

    private async Task<string> ReadResponseBodyAsync(Stream responseBody)
    {
        using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        return body.Length > _options.MaxResponseBodyLength
            ? body.Substring(0, _options.MaxResponseBodyLength) + "... (truncated)"
            : body;
    }
}
