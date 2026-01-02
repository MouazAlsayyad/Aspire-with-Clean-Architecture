using AspireApp.ApiService.Domain.Services;
using AspireApp.Domain.Shared.Common;
using AspireApp.Email.Domain.Entities;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.ValueObjects;

namespace AspireApp.Email.Domain.Services;

/// <summary>
/// Domain service (Manager) for Email entity.
/// Handles email-related domain logic and business rules.
/// </summary>
public class EmailManager : DomainService, IEmailManager
{
    private const int MaxAttachmentSizeMB = 25; // SendGrid limit
    private const int MaxSubjectLength = 255;
    private const int MaxBodyLength = 1048576; // 1MB

    public void ValidateEmailRequest(string toAddress, string subject, string body)
    {
        // Validate email address
        if (!EmailAddress.IsValidEmail(toAddress))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Invalid email address: {toAddress}"));
        }

        // Validate subject
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Email subject cannot be empty."));
        }

        if (subject.Length > MaxSubjectLength)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Email subject cannot exceed {MaxSubjectLength} characters."));
        }

        // Validate body
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Email body cannot be empty."));
        }

        if (body.Length > MaxBodyLength)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Email body cannot exceed {MaxBodyLength} characters."));
        }
    }

    public EmailLog CreateEmailLog(
        EmailType emailType,
        EmailPriority priority,
        string toAddress,
        string fromAddress,
        string subject,
        string? body = null,
        bool hasAttachments = false,
        string? bccAddresses = null,
        string? metadata = null)
    {
        var emailLog = new EmailLog(
            emailType,
            EmailStatus.Pending,
            priority,
            toAddress,
            fromAddress,
            subject,
            body)
        {
            HasAttachments = hasAttachments,
            BccAddresses = bccAddresses,
            Metadata = metadata
        };

        return emailLog;
    }

    public void ValidateAttachment(string? attachmentBase64, string fileName)
    {
        if (string.IsNullOrWhiteSpace(attachmentBase64))
        {
            return; // No attachment, nothing to validate
        }

        try
        {
            // Validate base64 string
            var bytes = Convert.FromBase64String(attachmentBase64);

            // Check file size
            var sizeInMB = bytes.Length / (1024.0 * 1024.0);
            if (sizeInMB > MaxAttachmentSizeMB)
            {
                throw new DomainException(
                    DomainErrors.General.InvalidInput(
                        $"Attachment size ({sizeInMB:F2} MB) exceeds maximum allowed size of {MaxAttachmentSizeMB} MB."));
            }
        }
        catch (FormatException)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Invalid attachment format. Must be a valid base64 string."));
        }

        // Validate file name
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Attachment file name cannot be empty."));
        }
    }

    public EmailPriority GetPriorityForEmailType(EmailType emailType)
    {
        return emailType switch
        {
            EmailType.OTP => EmailPriority.High,
            EmailType.PayoutOTP => EmailPriority.High,
            EmailType.ForgotPassword => EmailPriority.High,
            EmailType.PayoutConfirmation => EmailPriority.Normal,
            EmailType.PayoutRejection => EmailPriority.Normal,
            EmailType.Booking => EmailPriority.Normal,
            EmailType.CompletedBooking => EmailPriority.Normal,
            EmailType.Membership => EmailPriority.Normal,
            EmailType.Subscription => EmailPriority.Low,
            EmailType.Onboarding => EmailPriority.Low,
            _ => EmailPriority.Normal
        };
    }
}

