using AspireApp.Notifications.Domain.Enums;

namespace AspireApp.Notifications.Application.DTOs;

/// <summary>
/// DTO for notification send results
/// </summary>
public class NotificationResultDto
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
}

