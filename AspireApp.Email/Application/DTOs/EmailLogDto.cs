using AspireApp.Email.Domain.Enums;

namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for EmailLog entity
/// </summary>
public class EmailLogDto
{
    public Guid Id { get; set; }
    public EmailType EmailType { get; set; }
    public EmailStatus Status { get; set; }
    public EmailPriority Priority { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public string? MessageId { get; set; }
    public bool HasAttachments { get; set; }
    public DateTime CreationTime { get; set; }
}

