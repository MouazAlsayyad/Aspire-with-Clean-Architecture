using AspireApp.Notifications.Domain.Enums;

namespace AspireApp.Notifications.Application.DTOs;

/// <summary>
/// DTO for sending notifications
/// </summary>
public class SendNotificationDto
{
    /// <summary>
    /// Recipient identifier (email, phone, FCM token, or user ID)
    /// </summary>
    public string Recipient { get; set; } = string.Empty;
    
    /// <summary>
    /// Notification subject/title
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Notification body/message
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional metadata for the notification
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
    
    /// <summary>
    /// Channels through which to send the notification
    /// </summary>
    public NotificationChannel[] Channels { get; set; } = Array.Empty<NotificationChannel>();
}

