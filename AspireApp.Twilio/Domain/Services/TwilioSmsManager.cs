using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AspireApp.Twilio.Domain.Services;

/// <summary>
/// Domain service (Manager) for Twilio messaging operations.
/// Handles SMS/WhatsApp messaging-related domain logic and business rules.
/// </summary>
public class TwilioSmsManager : DomainService, ITwilioSmsManager
{
    private readonly IMessageRepository _messageRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly ITwilioClientService _twilioClientService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsManager> _logger;

    private readonly string _phoneNumber;
    private readonly string _whatsAppSender;
    private readonly string _senderName;

    public TwilioSmsManager(
        IMessageRepository messageRepository,
        IOtpRepository otpRepository,
        ITwilioClientService twilioClientService,
        IConfiguration configuration,
        ILogger<TwilioSmsManager> logger)
    {
        _messageRepository = messageRepository;
        _otpRepository = otpRepository;
        _twilioClientService = twilioClientService;
        _configuration = configuration;
        _logger = logger;

        _phoneNumber = _configuration["Twilio:PhoneNumber"] ?? throw new InvalidOperationException("Twilio:PhoneNumber configuration is required");
        _whatsAppSender = _configuration["Twilio:WhatsAppSender"] ?? throw new InvalidOperationException("Twilio:WhatsAppSender configuration is required");
        _senderName = _configuration["Twilio:SenderName"] ?? "Sender";
    }

    public async Task<Message> SendSmsAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(toPhoneNumber));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        var messageEntity = new Message(toPhoneNumber, message, MessageChannel.SMS);
        await _messageRepository.InsertAsync(messageEntity, cancellationToken);

        try
        {
            var messageSid = await _twilioClientService.SendSmsAsync(
                toPhoneNumber,
                _phoneNumber,
                $"{_senderName} :\n{message}",
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(messageSid))
            {
                messageEntity.MarkAsSent(messageSid);
                await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}, MessageSid: {MessageSid}", toPhoneNumber, messageSid);
            }
            else
            {
                messageEntity.MarkAsFailed("Failed to send SMS via Twilio");
                await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
                _logger.LogWarning("Failed to send SMS to {PhoneNumber}", toPhoneNumber);
            }
        }
        catch (Exception ex)
        {
            messageEntity.MarkAsFailed(ex.Message);
            await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", toPhoneNumber);
            throw;
        }

        return messageEntity;
    }

    public async Task<Message> SendWhatsAppAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(toPhoneNumber));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        var messageEntity = new Message(toPhoneNumber, message, MessageChannel.WhatsApp);
        await _messageRepository.InsertAsync(messageEntity, cancellationToken);

        try
        {
            var messageSid = await _twilioClientService.SendWhatsAppAsync(
                toPhoneNumber,
                _whatsAppSender,
                message,
                null,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(messageSid))
            {
                messageEntity.MarkAsSent(messageSid);
                await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
                _logger.LogInformation("WhatsApp message sent successfully to {PhoneNumber}, MessageSid: {MessageSid}", toPhoneNumber, messageSid);
            }
            else
            {
                messageEntity.MarkAsFailed("Failed to send WhatsApp message via Twilio");
                await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
                _logger.LogWarning("Failed to send WhatsApp message to {PhoneNumber}", toPhoneNumber);
            }
        }
        catch (Exception ex)
        {
            messageEntity.MarkAsFailed(ex.Message);
            await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
            _logger.LogError(ex, "Error sending WhatsApp message to {PhoneNumber}", toPhoneNumber);
            throw;
        }

        return messageEntity;
    }

    public async Task<Message> SendWhatsAppWithTemplateAsync(
        string toPhoneNumber,
        string templateId,
        Dictionary<string, object> templateVariables,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(toPhoneNumber));
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty", nameof(templateId));

        var templateVariablesJson = JsonConvert.SerializeObject(templateVariables);
        var messageEntity = new Message(toPhoneNumber, string.Empty, MessageChannel.WhatsApp, templateId, templateVariablesJson);
        await _messageRepository.InsertAsync(messageEntity, cancellationToken);

        try
        {
            var messageSid = await _twilioClientService.SendWhatsAppWithTemplateAsync(
                toPhoneNumber,
                _whatsAppSender,
                templateId,
                templateVariables,
                statusCallbackUrl,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(messageSid))
            {
                messageEntity.MarkAsSent(messageSid);
                await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
                _logger.LogInformation("WhatsApp template message sent successfully to {PhoneNumber}, MessageSid: {MessageSid}", toPhoneNumber, messageSid);
            }
            else
            {
                messageEntity.MarkAsFailed("Failed to send WhatsApp template message via Twilio");
                await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
                _logger.LogWarning("Failed to send WhatsApp template message to {PhoneNumber}", toPhoneNumber);
            }
        }
        catch (Exception ex)
        {
            messageEntity.MarkAsFailed(ex.Message);
            await _messageRepository.UpdateAsync(messageEntity, cancellationToken);
            _logger.LogError(ex, "Error sending WhatsApp template message to {PhoneNumber}", toPhoneNumber);
            throw;
        }

        return messageEntity;
    }

    public async Task<(Otp Otp, Message Message)> SendOtpAsync(
        string phoneNumber,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        // Generate OTP
        var otp = await GenerateOtpAsync(phoneNumber, cancellationToken: cancellationToken);
        await _otpRepository.InsertAsync(otp, cancellationToken);

        // Try WhatsApp first, fallback to SMS
        Message message;
        try
        {
            message = await SendWhatsAppOtpAsync(phoneNumber, name ?? "User", otp.Code, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp OTP failed for {PhoneNumber}, falling back to SMS", phoneNumber);
            message = await SendSmsOtpAsync(phoneNumber, otp.Code, cancellationToken);
        }

        return (otp, message);
    }

    public async Task<Message> SendWhatsAppOtpAsync(
        string phoneNumber,
        string name,
        string otp,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP cannot be empty", nameof(otp));

        // OTP WhatsApp template ContentSid: HXfe322c53f5050d0c47ac60d576ab8646
        const string otpTemplateId = "HXfe322c53f5050d0c47ac60d576ab8646";

        var templateVariables = new Dictionary<string, object>
        {
            { "otp", otp }
        };

        // Build status callback URL if not provided
        var callbackUrl = statusCallbackUrl ?? BuildStatusCallbackUrl();

        return await SendWhatsAppWithTemplateAsync(phoneNumber, otpTemplateId, templateVariables, callbackUrl, cancellationToken);
    }

    public async Task<Message> SendSmsOtpAsync(
        string phoneNumber,
        string otp,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP cannot be empty", nameof(otp));

        var message = $"{otp} is your verification code. For your security, do not share this code.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<bool> ValidateOtpAsync(
        string phoneNumber,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(otpCode))
            throw new ArgumentException("OTP code cannot be empty", nameof(otpCode));

        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        var otp = await _otpRepository.GetLatestValidOtpAsync(normalizedPhone, cancellationToken);

        if (otp == null)
        {
            _logger.LogWarning("No valid OTP found for phone number {PhoneNumber}", phoneNumber);
            return false;
        }

        if (otp.Code != otpCode)
        {
            _logger.LogWarning("Invalid OTP code for phone number {PhoneNumber}", phoneNumber);
            return false;
        }

        if (!otp.IsValid)
        {
            _logger.LogWarning("OTP is expired or already used for phone number {PhoneNumber}", phoneNumber);
            return false;
        }

        otp.MarkAsUsed();
        await _otpRepository.UpdateAsync(otp, cancellationToken);
        _logger.LogInformation("OTP validated successfully for phone number {PhoneNumber}", phoneNumber);

        return true;
    }

    public Task<Otp> GenerateOtpAsync(
        string phoneNumber,
        int expirationMinutes = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        var random = new Random();
        var otpCode = random.Next(1000, 9999).ToString(); // 4-digit OTP

        var otp = new Otp(phoneNumber, otpCode, expirationMinutes);
        return Task.FromResult(otp);
    }

    public async Task UpdateMessageStatusAsync(
        string messageSid,
        MessageStatus status,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageSid))
            throw new ArgumentException("Message SID cannot be empty", nameof(messageSid));

        var message = await _messageRepository.GetByMessageSidAsync(messageSid, cancellationToken);
        if (message == null)
        {
            _logger.LogWarning("Message not found for MessageSid: {MessageSid}", messageSid);
            return;
        }

        message.UpdateStatus(status, messageSid);
        if (status == MessageStatus.Failed && !string.IsNullOrWhiteSpace(failureReason))
        {
            message.MarkAsFailed(failureReason);
        }

        await _messageRepository.UpdateAsync(message, cancellationToken);
        _logger.LogInformation("Message status updated: MessageSid={MessageSid}, Status={Status}", messageSid, status);
    }

    public async Task<Message?> HandleWhatsAppFailureAsync(
        string messageSid,
        string failureReason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageSid))
            throw new ArgumentException("Message SID cannot be empty", nameof(messageSid));

        var message = await _messageRepository.GetByMessageSidAsync(messageSid, cancellationToken);
        if (message == null)
        {
            _logger.LogWarning("Message not found for MessageSid: {MessageSid}", messageSid);
            return null;
        }

        if (message.Channel != MessageChannel.WhatsApp)
        {
            _logger.LogWarning("Message {MessageSid} is not a WhatsApp message, cannot fallback to SMS", messageSid);
            return null;
        }

        // Update original message as failed
        message.MarkAsFailed(failureReason);
        await _messageRepository.UpdateAsync(message, cancellationToken);

        // Send SMS fallback
        try
        {
            var smsMessage = await SendSmsAsync(message.RecipientPhoneNumber, message.MessageBody, cancellationToken);
            _logger.LogInformation("WhatsApp failure handled, SMS fallback sent to {PhoneNumber}", message.RecipientPhoneNumber);
            return smsMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS fallback for WhatsApp message {MessageSid}", messageSid);
            return null;
        }
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        return phoneNumber.Replace(" ", "").Trim();
    }

    private string BuildStatusCallbackUrl()
    {
        // This should be configured or built from the current request context
        // For now, return a placeholder that should be configured in appsettings
        var baseUrl = _configuration["Twilio:StatusCallbackBaseUrl"] ?? "https://localhost:7000";
        return $"{baseUrl}/api/twilio/whatsapp-status";
    }
}

