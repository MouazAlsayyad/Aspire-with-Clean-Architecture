using AspireApp.ApiService.Domain.Common;

namespace AspireApp.ApiService.Notifications.Domain.Events;

/// <summary>
/// Domain event raised when a notification is created.
/// </summary>
public class NotificationCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the notification ID
    /// </summary>
    public Guid NotificationId { get; }

    /// <summary>
    /// Gets the user ID who will receive the notification
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationCreatedEvent"/> class.
    /// </summary>
    public NotificationCreatedEvent(Guid notificationId, Guid userId)
    {
        NotificationId = notificationId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

