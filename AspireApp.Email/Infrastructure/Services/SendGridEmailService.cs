using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Infrastructure.RefitClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspireApp.Email.Infrastructure.Services;

/// <summary>
/// SendGrid email service implementation (alternative to SMTP)
/// </summary>
public class SendGridEmailService : IEmailService
{
    private readonly ISendGridApi _sendGridApi;
    private readonly string _apiKey;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        ISendGridApi sendGridApi,
        IConfiguration configuration,
        ILogger<SendGridEmailService> logger)
    {
        _sendGridApi = sendGridApi;
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
            var personalization = new SendGridPersonalization
            {
                To = new List<SendGridEmailAddress> { new SendGridEmailAddress { Email = toAddress } }
            };

            if (bccAddresses != null && bccAddresses.Any())
            {
                personalization.Bcc = bccAddresses.Select(bcc => new SendGridEmailAddress { Email = bcc }).ToList();
            }

            var request = new SendGridMessageRequest
            {
                Personalizations = new List<SendGridPersonalization> { personalization },
                From = new SendGridEmailAddress { Email = fromAddress, Name = fromName },
                Subject = subject,
                Content = new List<SendGridContent>
                {
                    new SendGridContent { Type = "text/html", Value = htmlContent }
                }
            };

            if (!string.IsNullOrEmpty(plainTextContent))
            {
                request.Content.Insert(0, new SendGridContent { Type = "text/plain", Value = plainTextContent });
            }

            // Add attachments if provided
            if (attachments != null && attachments.Any())
            {
                request.Attachments = attachments.Select(a => new SendGridAttachment
                {
                    Content = a.ContentBase64,
                    Filename = a.FileName,
                    Type = a.ContentType,
                    Disposition = a.Disposition
                }).ToList();
            }

            var response = await _sendGridApi.SendEmailAsync(request, $"Bearer {_apiKey}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.TryGetValues("X-Message-Id", out var values) ? values.FirstOrDefault() : null;
                _logger.LogInformation(
                    "Email sent successfully to {ToAddress}. Subject: {Subject}. MessageId: {MessageId}",
                    toAddress, subject, messageId);
                return (true, messageId, null);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
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

