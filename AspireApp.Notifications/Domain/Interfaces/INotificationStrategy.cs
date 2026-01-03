using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Models;

namespace AspireApp.Notifications.Domain.Interfaces;

/// <summary>
/// Base interface for notification strategies
/// </summary>
public interface INotificationStrategy
{
    /// <summary>
    /// The channel this strategy handles
    /// </summary>
    NotificationChannel Channel { get; }
    
    /// <summary>
    /// Sends a notification using this strategy
    /// </summary>
    Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}

