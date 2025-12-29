namespace AspireApp.ApiService.Domain.Common;

/// <summary>
/// Domain event raised when an entity is deleted.
/// Inherits from <see cref="EntityChangedEvent"/>.
/// </summary>
public class EntityDeletedEvent : EntityChangedEvent
{
    /// <summary>
    /// Gets the properties of the deleted entity (captured before deletion).
    /// </summary>
    public Dictionary<string, object?> Properties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeletedEvent"/> class.
    /// </summary>
    /// <param name="entityId">The unique identifier of the deleted entity.</param>
    /// <param name="entityType">The type name of the deleted entity.</param>
    /// <param name="properties">The properties of the deleted entity (captured before deletion).</param>
    public EntityDeletedEvent(Guid entityId, string entityType, Dictionary<string, object?> properties)
        : base(entityId, entityType)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }
}
