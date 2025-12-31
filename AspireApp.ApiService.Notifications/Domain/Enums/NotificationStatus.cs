namespace AspireApp.ApiService.Notifications.Domain.Enums;

/// <summary>
/// Status of notifications
/// </summary>
public enum NotificationStatus
{
    Pending = 0,     // Notification created but not sent
    Sent = 1,        // Notification successfully sent
    Failed = 2,      // Notification failed to send
    Cancelled = 3    // Notification cancelled
}

