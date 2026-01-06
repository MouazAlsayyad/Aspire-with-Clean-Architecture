using AspireApp.ApiService.Infrastructure.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;

namespace AspireApp.ApiService.Infrastructure.Tests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;
    private readonly RequestLoggingMiddleware _sut;

    public RequestLoggingMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<RequestLoggingMiddleware>>();
        _options = new RequestLoggingOptions();
        _sut = new RequestLoggingMiddleware(_next, _logger, _options);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequest_WhenPathIsNotExcluded()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        await _next.Received(1).Invoke(context);
        _logger.ReceivedWithAnyArgs(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipLogging_WhenPathIsExcluded()
    {
        // Arrange
        _options.ExcludedPaths.Add("/health");
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        await _next.Received(1).Invoke(context);
        _logger.DidNotReceiveWithAnyArgs().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestBody_WhenEnabled()
    {
        // Arrange
        _options.LogRequestBody = true;
        var context = new DefaultHttpContext();
        var body = "test body";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.Body = stream;
        context.Request.ContentLength = body.Length;
        context.Request.Path = "/api/test";

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        _logger.Received().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains(body)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
