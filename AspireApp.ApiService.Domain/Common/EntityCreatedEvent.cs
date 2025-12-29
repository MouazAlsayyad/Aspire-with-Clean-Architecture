namespace AspireApp.ApiService.Domain.Common;

/// <summary>
/// Domain event raised when an entity is created.
/// Inherits from <see cref="EntityChangedEvent"/>.
/// </summary>
public class EntityCreatedEvent : EntityChangedEvent
{
    /// <summary>
    /// Gets the properties of the created entity.
    /// </summary>
    public Dictionary<string, object?> Properties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreatedEvent"/> class.
    /// </summary>
    /// <param name="entityId">The unique identifier of the created entity.</param>
    /// <param name="entityType">The type name of the created entity.</param>
    /// <param name="properties">The properties of the created entity.</param>
    public EntityCreatedEvent(Guid entityId, string entityType, Dictionary<string, object?> properties)
        : base(entityId, entityType)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }
}
