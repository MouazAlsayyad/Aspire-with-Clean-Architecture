using AspireApp.Domain.Shared.Entities;
using AspireApp.Email.Domain.Enums;

namespace AspireApp.Email.Domain.Entities;

/// <summary>
/// Represents an audit log entry for sent emails
/// </summary>
public class EmailLog : BaseEntity
{
    /// <summary>
    /// Type of email sent
    /// </summary>
    public EmailType EmailType { get; set; }

    /// <summary>
    /// Current status of the email
    /// </summary>
    public EmailStatus Status { get; set; }

    /// <summary>
    /// Priority of the email
    /// </summary>
    public EmailPriority Priority { get; set; }

    /// <summary>
    /// Recipient email address
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body (HTML content)
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Timestamp when email was sent (or attempted)
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// SendGrid message ID (for tracking)
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// BCC recipients (comma-separated)
    /// </summary>
    public string? BccAddresses { get; set; }

    /// <summary>
    /// Indicates if email has attachments
    /// </summary>
    public bool HasAttachments { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    protected EmailLog() { }

    public EmailLog(
        EmailType emailType,
        EmailStatus status,
        EmailPriority priority,
        string toAddress,
        string fromAddress,
        string subject,
        string? body = null)
    {
        EmailType = emailType;
        Status = status;
        Priority = priority;
        ToAddress = toAddress;
        FromAddress = fromAddress;
        Subject = subject;
        Body = body;
        RetryCount = 0;
    }

    /// <summary>
    /// Mark email as sent successfully
    /// </summary>
    public void MarkAsSent(string? messageId = null)
    {
        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        MessageId = messageId;
        ErrorMessage = null;
    }

    /// <summary>
    /// Mark email as failed
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = EmailStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    /// <summary>
    /// Mark email as queued
    /// </summary>
    public void MarkAsQueued()
    {
        Status = EmailStatus.Queued;
    }
}

