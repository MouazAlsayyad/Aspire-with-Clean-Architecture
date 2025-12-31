using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Notifications.Domain.Enums;

namespace AspireApp.ApiService.Notifications.Domain.Entities;

/// <summary>
/// Notification aggregate root.
/// Represents a notification sent to a user via Firebase Cloud Messaging.
/// </summary>
public class Notification : BaseEntity, IAggregateRoot
{
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public NotificationStatus Status { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string TitleAr { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string MessageAr { get; private set; } = string.Empty;
    public string? ActionUrl { get; private set; }
    public Guid UserId { get; private set; }

    // Navigation property for EF Core (cross-aggregate reference)
    public User User { get; private set; } = null!;

    // Private constructor for EF Core
    private Notification() { }

    public Notification(
        NotificationType type,
        NotificationPriority priority,
        string title,
        string titleAr,
        string message,
        string messageAr,
        Guid userId,
        string? actionUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        Type = type;
        Priority = priority;
        Title = title;
        TitleAr = titleAr ?? string.Empty;
        Message = message;
        MessageAr = messageAr ?? string.Empty;
        UserId = userId;
        ActionUrl = actionUrl;
        Status = NotificationStatus.Pending;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        SetLastModificationTime();
    }

    public void MarkAsUnread()
    {
        if (!IsRead)
            return;

        IsRead = false;
        ReadAt = null;
        SetLastModificationTime();
    }

    public void UpdateStatus(NotificationStatus status)
    {
        Status = status;
        SetLastModificationTime();
    }
}

