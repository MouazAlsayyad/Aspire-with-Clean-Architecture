using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Models;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using Microsoft.Extensions.Logging;

namespace AspireApp.Notifications.Infrastructure.Strategies;

/// <summary>
/// Strategy for sending notifications via Twilio SMS
/// </summary>
public class TwilioSmsNotificationStrategy : ITwilioSmsNotificationStrategy
{
    private readonly SendSmsUseCase _sendSmsUseCase;
    private readonly ILogger<TwilioSmsNotificationStrategy> _logger;

    public NotificationChannel Channel => NotificationChannel.TwilioSms;

    public TwilioSmsNotificationStrategy(
        SendSmsUseCase sendSmsUseCase,
        ILogger<TwilioSmsNotificationStrategy> logger)
    {
        _sendSmsUseCase = sendSmsUseCase;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smsDto = new SendSmsDto(
                ToPhoneNumber: request.Recipient,
                Message: $"{request.Subject}\n\n{request.Body}");

            var result = await _sendSmsUseCase.ExecuteAsync(smsDto, cancellationToken);

            if (result.IsSuccess)
            {
                return NotificationResult.Successful(
                    NotificationChannel.TwilioSms,
                    result.Value?.MessageSid);
            }

            return NotificationResult.Failed(
                NotificationChannel.TwilioSms,
                result.Error?.Message ?? "Failed to send SMS");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS notification to {Recipient}", request.Recipient);
            return NotificationResult.Failed(NotificationChannel.TwilioSms, ex.Message);
        }
    }
}

