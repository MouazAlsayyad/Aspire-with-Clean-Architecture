using AspireApp.Email.Domain.Enums;

namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Email service interface for sending emails via SendGrid
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously
    /// </summary>
    Task<(bool Success, string? MessageId, string? Error)> SendEmailAsync(
        string toAddress,
        string fromAddress,
        string fromName,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        List<EmailAttachment>? attachments = null,
        List<string>? bccAddresses = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an email attachment
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public string Disposition { get; set; } = "attachment";
}

