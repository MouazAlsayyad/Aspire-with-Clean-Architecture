using AspireApp.Email.Domain.Interfaces;
using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspireApp.Notifications.Infrastructure.Strategies;

/// <summary>
/// Strategy for sending notifications via Email
/// </summary>
public class EmailNotificationStrategy : IEmailNotificationStrategy
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationStrategy> _logger;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public NotificationChannel Channel => NotificationChannel.Email;

    public EmailNotificationStrategy(
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<EmailNotificationStrategy> logger)
    {
        _emailService = emailService;
        _logger = logger;
        _senderEmail = configuration["Email:SenderEmail"] ?? "noreply@example.com";
        _senderName = configuration["Email:SenderName"] ?? "Notification Service";
    }

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a simple HTML email body
            var htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>{request.Subject}</h2>
                        </div>
                        <div class='content'>
                            {request.Body.Replace("\n", "<br>")}
                        </div>
                    </div>
                </body>
                </html>";

            var (success, messageId, error) = await _emailService.SendEmailAsync(
                request.Recipient,
                _senderEmail,
                _senderName,
                request.Subject,
                htmlBody,
                cancellationToken: cancellationToken);

            if (success)
            {
                return NotificationResult.Successful(NotificationChannel.Email, messageId);
            }

            return NotificationResult.Failed(
                NotificationChannel.Email,
                error ?? "Failed to send email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification to {Recipient}", request.Recipient);
            return NotificationResult.Failed(NotificationChannel.Email, ex.Message);
        }
    }
}

