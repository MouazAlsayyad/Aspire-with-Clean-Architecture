using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Domain.Entities;
using AspireApp.Email.Domain.Enums;

namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Domain service interface for email business logic
/// </summary>
public interface IEmailManager : IDomainService
{
    /// <summary>
    /// Validates email request parameters
    /// </summary>
    void ValidateEmailRequest(string toAddress, string subject, string body);

    /// <summary>
    /// Creates an email log entry
    /// </summary>
    EmailLog CreateEmailLog(
        EmailType emailType,
        EmailPriority priority,
        string toAddress,
        string fromAddress,
        string subject,
        string? body = null,
        bool hasAttachments = false,
        string? bccAddresses = null,
        string? metadata = null);

    /// <summary>
    /// Validates attachment data
    /// </summary>
    void ValidateAttachment(string? attachmentBase64, string fileName);

    /// <summary>
    /// Gets email priority based on email type
    /// </summary>
    EmailPriority GetPriorityForEmailType(EmailType emailType);
}

