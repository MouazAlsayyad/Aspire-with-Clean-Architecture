using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Models;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using Microsoft.Extensions.Logging;

namespace AspireApp.Notifications.Infrastructure.Strategies;

/// <summary>
/// Strategy for sending notifications via Twilio WhatsApp
/// </summary>
public class TwilioWhatsAppNotificationStrategy : ITwilioWhatsAppNotificationStrategy
{
    private readonly SendWhatsAppUseCase _sendWhatsAppUseCase;
    private readonly ILogger<TwilioWhatsAppNotificationStrategy> _logger;

    public NotificationChannel Channel => NotificationChannel.TwilioWhatsApp;

    public TwilioWhatsAppNotificationStrategy(
        SendWhatsAppUseCase sendWhatsAppUseCase,
        ILogger<TwilioWhatsAppNotificationStrategy> logger)
    {
        _sendWhatsAppUseCase = sendWhatsAppUseCase;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var whatsAppDto = new SendWhatsAppDto(
                ToPhoneNumber: request.Recipient,
                Message: $"*{request.Subject}*\n\n{request.Body}");

            var result = await _sendWhatsAppUseCase.ExecuteAsync(whatsAppDto, cancellationToken);

            if (result.IsSuccess)
            {
                return NotificationResult.Successful(
                    NotificationChannel.TwilioWhatsApp,
                    result.Value?.MessageSid);
            }

            return NotificationResult.Failed(
                NotificationChannel.TwilioWhatsApp,
                result.Error?.Message ?? "Failed to send WhatsApp message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp notification to {Recipient}", request.Recipient);
            return NotificationResult.Failed(NotificationChannel.TwilioWhatsApp, ex.Message);
        }
    }
}

