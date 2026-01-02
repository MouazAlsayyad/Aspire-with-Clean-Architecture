using AspireApp.Email.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace AspireApp.Email.Infrastructure.Services;

/// <summary>
/// SMTP email service implementation using System.Net.Mail
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly bool _enableSsl;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IConfiguration configuration,
        ILogger<SmtpEmailService> logger)
    {
        _smtpHost = configuration["Email:SMTP:Host"]
            ?? throw new InvalidOperationException("SMTP Host not found in configuration.");
        _smtpPort = configuration.GetValue<int>("Email:SMTP:Port", 587);
        _smtpUsername = configuration["Email:SMTP:Username"]
            ?? throw new InvalidOperationException("SMTP Username not found in configuration.");
        _smtpPassword = configuration["Email:SMTP:Password"]
            ?? throw new InvalidOperationException("SMTP Password not found in configuration.");
        _enableSsl = configuration.GetValue<bool>("Email:SMTP:EnableSsl", true);
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
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds timeout
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            // Add recipient
            mailMessage.To.Add(new MailAddress(toAddress));

            // Add BCC addresses if provided
            if (bccAddresses != null && bccAddresses.Any())
            {
                foreach (var bcc in bccAddresses)
                {
                    mailMessage.Bcc.Add(new MailAddress(bcc));
                }
            }

            // Add plain text alternative if provided
            if (!string.IsNullOrEmpty(plainTextContent))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(
                    plainTextContent,
                    null,
                    "text/plain");
                mailMessage.AlternateViews.Add(plainView);

                var htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlContent,
                    null,
                    "text/html");
                mailMessage.AlternateViews.Add(htmlView);
            }

            // Add attachments if provided
            if (attachments != null && attachments.Any())
            {
                foreach (var attachment in attachments)
                {
                    var bytes = Convert.FromBase64String(attachment.ContentBase64);
                    var stream = new MemoryStream(bytes);
                    var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                    mailMessage.Attachments.Add(mailAttachment);
                }
            }

            // Send email
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            // Generate a message ID for tracking
            var messageId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Email sent successfully to {ToAddress}. Subject: {Subject}. MessageId: {MessageId}",
                toAddress, subject, messageId);

            return (true, messageId, null);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex,
                "SMTP error while sending email to {ToAddress}. Subject: {Subject}. StatusCode: {StatusCode}",
                toAddress, subject, ex.StatusCode);
            return (false, null, $"SMTP Error: {ex.Message} (StatusCode: {ex.StatusCode})");
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

