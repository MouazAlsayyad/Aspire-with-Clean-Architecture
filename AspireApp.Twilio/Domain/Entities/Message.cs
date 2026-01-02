using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Twilio.Domain.Enums;

namespace AspireApp.Twilio.Domain.Entities;

/// <summary>
/// Message aggregate root.
/// Represents a message sent via Twilio (SMS or WhatsApp).
/// </summary>
public class Message : BaseEntity, IAggregateRoot
{
    public string RecipientPhoneNumber { get; private set; } = string.Empty;
    public string MessageBody { get; private set; } = string.Empty;
    public MessageChannel Channel { get; private set; }
    public MessageStatus Status { get; private set; }
    public string? MessageSid { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? TemplateId { get; private set; }
    public string? TemplateVariables { get; private set; }

    // Private constructor for EF Core
    private Message() { }

    public Message(
        string recipientPhoneNumber,
        string messageBody,
        MessageChannel channel,
        string? templateId = null,
        string? templateVariables = null)
    {
        if (string.IsNullOrWhiteSpace(recipientPhoneNumber))
            throw new ArgumentException("Recipient phone number cannot be empty", nameof(recipientPhoneNumber));
        if (string.IsNullOrWhiteSpace(messageBody) && string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Message body or template ID must be provided", nameof(messageBody));

        RecipientPhoneNumber = NormalizePhoneNumber(recipientPhoneNumber);
        MessageBody = messageBody ?? string.Empty;
        Channel = channel;
        Status = MessageStatus.Queued;
        TemplateId = templateId;
        TemplateVariables = templateVariables;
    }

    public void MarkAsSent(string messageSid)
    {
        if (string.IsNullOrWhiteSpace(messageSid))
            throw new ArgumentException("Message SID cannot be empty", nameof(messageSid));

        Status = MessageStatus.Sent;
        MessageSid = messageSid;
        SentAt = DateTime.UtcNow;
        SetLastModificationTime();
    }

    public void MarkAsDelivered()
    {
        if (Status == MessageStatus.Delivered)
            return;

        Status = MessageStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        SetLastModificationTime();
    }

    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));

        Status = MessageStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        SetLastModificationTime();
    }

    public void UpdateStatus(MessageStatus status, string? messageSid = null)
    {
        Status = status;
        if (!string.IsNullOrWhiteSpace(messageSid))
        {
            MessageSid = messageSid;
        }

        switch (status)
        {
            case MessageStatus.Sent:
                if (SentAt == null)
                    SentAt = DateTime.UtcNow;
                break;
            case MessageStatus.Delivered:
                if (DeliveredAt == null)
                    DeliveredAt = DateTime.UtcNow;
                break;
            case MessageStatus.Failed:
                if (FailedAt == null)
                    FailedAt = DateTime.UtcNow;
                break;
        }

        SetLastModificationTime();
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove spaces and normalize phone number format
        return phoneNumber.Replace(" ", "").Trim();
    }
}

