namespace AspireApp.ApiService.Domain.Common;

/// <summary>
/// Base class for entity change domain events.
/// All entity change events (Created, Updated, Deleted) inherit from this class.
/// </summary>
public abstract class EntityChangedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the entity that was changed.
    /// </summary>
    public Guid EntityId { get; }

    /// <summary>
    /// Gets the type name of the entity that was changed.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityChangedEvent"/> class.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="entityType">The type name of the entity.</param>
    protected EntityChangedEvent(Guid entityId, string entityType)
    {
        EntityId = entityId;
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        OccurredOn = DateTime.UtcNow;
    }
}

