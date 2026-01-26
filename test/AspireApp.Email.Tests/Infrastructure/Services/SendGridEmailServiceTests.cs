using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Infrastructure.RefitClients;
using AspireApp.Email.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace AspireApp.Email.Tests.Infrastructure.Services;

public class SendGridEmailServiceTests
{
    private readonly ISendGridApi _sendGridApi;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly SendGridEmailService _sut;

    public SendGridEmailServiceTests()
    {
        _sendGridApi = Substitute.For<ISendGridApi>();
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<SendGridEmailService>>();

        _configuration["Email:SendGrid:ApiKey"].Returns("test_key");

        _sut = new SendGridEmailService(
            _sendGridApi,
            _configuration,
            _logger);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldCallApiRefitClient()
    {
        // Arrange
        var to = "test@example.com";
        var from = "sender@example.com";
        var fromName = "Sender";
        var subject = "Test Subject";
        var content = "HTML Content";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.Accepted);
        responseMessage.Headers.Add("X-Message-Id", "msg_123");

        _sendGridApi.SendEmailAsync(
            Arg.Is<SendGridMessageRequest>(r =>
                r.Personalizations.First().To.First().Email == to &&
                r.From.Email == from &&
                r.Subject == subject),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(responseMessage);

        // Act
        var result = await _sut.SendEmailAsync(to, from, fromName, subject, content);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("msg_123", result.MessageId);
        await _sendGridApi.Received(1).SendEmailAsync(
            Arg.Any<SendGridMessageRequest>(),
            "Bearer test_key",
            Arg.Any<CancellationToken>());
    }
}
