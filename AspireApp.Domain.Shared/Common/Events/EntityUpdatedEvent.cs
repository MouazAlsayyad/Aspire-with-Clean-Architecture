using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Common.Events;

/// <summary>
/// Domain event raised when an entity is updated.
/// Inherits from <see cref="EntityChangedEvent"/>.
/// </summary>
public class EntityUpdatedEvent : EntityChangedEvent
{
    /// <summary>
    /// Gets the dictionary of changed properties, where each entry contains the property name
    /// and its corresponding <see cref="PropertyChange"/> with old and new values.
    /// </summary>
    public Dictionary<string, PropertyChange> ChangedProperties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdatedEvent"/> class.
    /// </summary>
    /// <param name="entityId">The unique identifier of the updated entity.</param>
    /// <param name="entityType">The type name of the updated entity.</param>
    /// <param name="changedProperties">The dictionary of changed properties with their old and new values.</param>
    public EntityUpdatedEvent(Guid entityId, string entityType, Dictionary<string, PropertyChange> changedProperties)
        : base(entityId, entityType)
    {
        ChangedProperties = changedProperties ?? throw new ArgumentNullException(nameof(changedProperties));
    }
}

