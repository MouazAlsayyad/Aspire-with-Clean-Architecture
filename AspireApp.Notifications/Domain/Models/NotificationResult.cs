using AspireApp.Notifications.Domain.Enums;

namespace AspireApp.Notifications.Domain.Models;

/// <summary>
/// Result of a notification sending operation
/// </summary>
public class NotificationResult
{
    /// <summary>
    /// Channel through which the notification was sent
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Whether the notification was sent successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the notification failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// External reference/ID from the notification provider
    /// </summary>
    public string? ExternalReference { get; set; }
    
    /// <summary>
    /// Additional metadata about the send operation
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    public static NotificationResult Successful(NotificationChannel channel, string? externalReference = null)
    {
        return new NotificationResult
        {
            Channel = channel,
            Success = true,
            ExternalReference = externalReference
        };
    }

    public static NotificationResult Failed(NotificationChannel channel, string errorMessage)
    {
        return new NotificationResult
        {
            Channel = channel,
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

