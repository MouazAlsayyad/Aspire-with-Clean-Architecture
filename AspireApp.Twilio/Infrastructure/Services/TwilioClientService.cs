using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AspireApp.Twilio.Infrastructure.Services;

/// <summary>
/// Twilio API client service implementation.
/// Handles direct communication with Twilio API.
/// </summary>
public class TwilioClientService : ITwilioClientService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioClientService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;

    public TwilioClientService(
        IConfiguration configuration,
        ILogger<TwilioClientService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _accountSid = _configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid configuration is required");
        _authToken = _configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio:AuthToken configuration is required");

        InitializeTwilio();
    }

    private void InitializeTwilio()
    {
        try
        {
            TwilioClient.Init(_accountSid, _authToken);
            _logger.LogInformation("Twilio client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Twilio client");
            throw;
        }
    }

    public async Task<string?> SendSmsAsync(
        string toPhoneNumber,
        string fromPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(fromPhoneNumber),
                Body = message
            };

            var messageResource = await MessageResource.CreateAsync(messageOptions);
            _logger.LogInformation("SMS sent successfully. MessageSid: {MessageSid}, Status: {Status}",
                messageResource.Sid, messageResource.Status);

            return messageResource.Sid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", toPhoneNumber);
            return null;
        }
    }

    public async Task<string?> SendWhatsAppAsync(
        string toPhoneNumber,
        string fromWhatsAppNumber,
        string message,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedTo = NormalizePhoneNumber(toPhoneNumber);
            var normalizedFrom = NormalizePhoneNumber(fromWhatsAppNumber);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{normalizedTo}"))
            {
                From = new PhoneNumber($"whatsapp:{normalizedFrom}"),
                Body = message
            };

            if (!string.IsNullOrWhiteSpace(statusCallbackUrl))
            {
                messageOptions.StatusCallback = new Uri(statusCallbackUrl);
            }

            var messageResource = await MessageResource.CreateAsync(messageOptions);
            _logger.LogInformation("WhatsApp message sent successfully. MessageSid: {MessageSid}, Status: {Status}", 
                messageResource.Sid, messageResource.Status);

            return messageResource.Sid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {PhoneNumber}", toPhoneNumber);
            return null;
        }
    }

    public async Task<string?> SendWhatsAppWithTemplateAsync(
        string toPhoneNumber,
        string fromWhatsAppNumber,
        string templateId,
        Dictionary<string, object> templateVariables,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedTo = NormalizePhoneNumber(toPhoneNumber);
            var normalizedFrom = NormalizePhoneNumber(fromWhatsAppNumber);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{normalizedTo}"))
            {
                From = new PhoneNumber($"whatsapp:{normalizedFrom}"),
                ContentSid = templateId
            };

            // Convert template variables to JSON string
            var contentVariables = JsonConvert.SerializeObject(templateVariables);
            messageOptions.ContentVariables = contentVariables;

            if (!string.IsNullOrWhiteSpace(statusCallbackUrl))
            {
                messageOptions.StatusCallback = new Uri(statusCallbackUrl);
            }

            var messageResource = await MessageResource.CreateAsync(messageOptions);
            _logger.LogInformation("WhatsApp template message sent successfully. MessageSid: {MessageSid}, Status: {Status}", 
                messageResource.Sid, messageResource.Status);

            return messageResource.Sid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp template message to {PhoneNumber}", toPhoneNumber);
            return null;
        }
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        return phoneNumber.Replace(" ", "").Trim();
    }
}

