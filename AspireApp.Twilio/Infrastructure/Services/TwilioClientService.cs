using System.Text;
using AspireApp.Twilio.Domain.Interfaces;
using AspireApp.Twilio.Infrastructure.RefitClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AspireApp.Twilio.Infrastructure.Services;

/// <summary>
/// Twilio API client service implementation.
/// Handles direct communication with Twilio API via Refit.
/// </summary>
public class TwilioClientService : ITwilioClientService
{
    private readonly ITwilioApi _twilioApi;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioClientService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _authHeader;

    public TwilioClientService(
        ITwilioApi twilioApi,
        IConfiguration configuration,
        ILogger<TwilioClientService> logger)
    {
        _twilioApi = twilioApi;
        _configuration = configuration;
        _logger = logger;

        _accountSid = _configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid configuration is required");
        _authToken = _configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio:AuthToken configuration is required");

        var authBytes = Encoding.UTF8.GetBytes($"{_accountSid}:{_authToken}");
        _authHeader = $"Basic {Convert.ToBase64String(authBytes)}";
    }

    public async Task<string?> SendSmsAsync(
        string toPhoneNumber,
        string fromPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new TwilioMessageRequest
            {
                To = toPhoneNumber,
                From = fromPhoneNumber,
                Body = message
            };

            var response = await _twilioApi.SendMessageAsync(_accountSid, request, _authHeader, cancellationToken);

            _logger.LogInformation("SMS sent successfully. MessageSid: {MessageSid}, Status: {Status}",
                response.Sid, response.Status);

            return response.Sid;
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

            var request = new TwilioMessageRequest
            {
                To = $"whatsapp:{normalizedTo}",
                From = $"whatsapp:{normalizedFrom}",
                Body = message,
                StatusCallback = statusCallbackUrl
            };

            var response = await _twilioApi.SendMessageAsync(_accountSid, request, _authHeader, cancellationToken);

            _logger.LogInformation("WhatsApp message sent successfully. MessageSid: {MessageSid}, Status: {Status}",
                response.Sid, response.Status);

            return response.Sid;
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

            // Convert template variables to JSON string
            var contentVariables = JsonConvert.SerializeObject(templateVariables);

            var request = new TwilioMessageRequest
            {
                To = $"whatsapp:{normalizedTo}",
                From = $"whatsapp:{normalizedFrom}",
                ContentSid = templateId,
                ContentVariables = contentVariables,
                StatusCallback = statusCallbackUrl
            };

            var response = await _twilioApi.SendMessageAsync(_accountSid, request, _authHeader, cancellationToken);

            _logger.LogInformation("WhatsApp template message sent successfully. MessageSid: {MessageSid}, Status: {Status}",
                response.Sid, response.Status);

            return response.Sid;
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

