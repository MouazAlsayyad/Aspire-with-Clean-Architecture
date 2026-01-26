using AspireApp.Twilio.Infrastructure.RefitClients;
using AspireApp.Twilio.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AspireApp.Twilio.Tests.Infrastructure.Services;

public class TwilioClientServiceTests
{
    private readonly ITwilioApi _twilioApi;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioClientService> _logger;
    private readonly TwilioClientService _sut;

    public TwilioClientServiceTests()
    {
        _twilioApi = Substitute.For<ITwilioApi>();
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<TwilioClientService>>();

        _configuration["Twilio:AccountSid"].Returns("test_sid");
        _configuration["Twilio:AuthToken"].Returns("test_token");

        _sut = new TwilioClientService(
            _twilioApi,
            _configuration,
            _logger);
    }

    [Fact]
    public async Task SendSmsAsync_ShouldCallApiRefitClient()
    {
        // Arrange
        var to = "+1234567890";
        var from = "+0987654321";
        var message = "Test SMS";

        _twilioApi.SendMessageAsync(
            "test_sid",
            Arg.Is<TwilioMessageRequest>(r => r.To == to && r.From == from && r.Body == message),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new TwilioMessageResponse { Sid = "SM123", Status = "queued" });

        // Act
        var result = await _sut.SendSmsAsync(to, from, message);

        // Assert
        Assert.Equal("SM123", result);
        await _twilioApi.Received(1).SendMessageAsync(
            "test_sid",
            Arg.Any<TwilioMessageRequest>(),
            Arg.Is<string>(s => s.StartsWith("Basic ")),
            Arg.Any<CancellationToken>());
    }
}
