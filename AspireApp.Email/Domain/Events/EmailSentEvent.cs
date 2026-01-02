using AspireApp.Domain.Shared.Common;

namespace AspireApp.Email.Domain.Events;

/// <summary>
/// Domain event raised when an email is successfully sent
/// </summary>
public class EmailSentEvent : IDomainEvent
{
    public Guid EmailLogId { get; }
    public string ToAddress { get; }
    public string Subject { get; }
    public DateTime SentAt { get; }
    public DateTime OccurredOn { get; }

    public EmailSentEvent(Guid emailLogId, string toAddress, string subject, DateTime sentAt)
    {
        EmailLogId = emailLogId;
        ToAddress = toAddress;
        Subject = subject;
        SentAt = sentAt;
        OccurredOn = DateTime.UtcNow;
    }
}

