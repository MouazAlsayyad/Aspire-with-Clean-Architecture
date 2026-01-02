using AspireApp.Email.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AspireApp.Email.Infrastructure.Services;

/// <summary>
/// SendGrid email service implementation (alternative to SMTP)
/// </summary>
public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IConfiguration configuration,
        ILogger<SendGridEmailService> logger)
    {
        _apiKey = configuration["Email:SendGrid:ApiKey"]
            ?? throw new InvalidOperationException("SendGrid API key not found in configuration.");
        _logger = logger;
    }

    public async Task<(bool Success, string? MessageId, string? Error)> SendEmailAsync(
        string toAddress,
        string fromAddress,
        string fromName,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        List<EmailAttachment>? attachments = null,
        List<string>? bccAddresses = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress(fromAddress, fromName);
            var to = new EmailAddress(toAddress);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent ?? string.Empty,
                htmlContent);

            // Add attachments if provided
            if (attachments != null && attachments.Any())
            {
                foreach (var attachment in attachments)
                {
                    msg.AddAttachment(new Attachment
                    {
                        Content = attachment.ContentBase64,
                        Filename = attachment.FileName,
                        Type = attachment.ContentType,
                        Disposition = attachment.Disposition
                    });
                }
            }

            // Add BCC addresses if provided
            if (bccAddresses != null && bccAddresses.Any())
            {
                foreach (var bcc in bccAddresses)
                {
                    msg.AddBcc(new EmailAddress(bcc));
                }
            }

            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.GetValues("X-Message-Id")?.FirstOrDefault();
                _logger.LogInformation(
                    "Email sent successfully to {ToAddress}. Subject: {Subject}. MessageId: {MessageId}",
                    toAddress, subject, messageId);
                return (true, messageId, null);
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send email to {ToAddress}. Status: {StatusCode}. Error: {Error}",
                    toAddress, response.StatusCode, errorBody);
                return (false, null, $"SendGrid returned {response.StatusCode}: {errorBody}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception occurred while sending email to {ToAddress}. Subject: {Subject}",
                toAddress, subject);
            return (false, null, ex.Message);
        }
    }
}

