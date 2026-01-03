using AspireApp.ApiService.Domain.ActivityLogs.Enums;
using AspireApp.ApiService.Domain.ActivityLogs.Interfaces;
using AspireApp.Domain.Shared.Common.Events;

namespace AspireApp.ApiService.Infrastructure.DomainEvents;

/// <summary>
/// Handles entity change domain events and logs them as activities
/// TEMPORARILY DISABLED - This handler is causing circular dependency deadlock
/// The activity logger calls SaveChangesAsync which triggers more domain events, creating an infinite loop
/// </summary>

// ENTIRE CLASS COMMENTED OUT TO PREVENT DEADLOCK
// Uncomment and fix the circular dependency issue before re-enabling

/*
public class EntityChangeTrackingHandler :
    IDomainEventHandler<EntityCreatedEvent>,
    IDomainEventHandler<EntityUpdatedEvent>,
    IDomainEventHandler<EntityDeletedEvent>
{
    private readonly IActivityLogger _activityLogger;

    public EntityChangeTrackingHandler(IActivityLogger activityLogger)
    {
        _activityLogger = activityLogger ?? throw new ArgumentNullException(nameof(activityLogger));
    }

    public async Task HandleAsync(EntityCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var description = $"Entity {domainEvent.EntityType} with ID {domainEvent.EntityId} was created";
            
            var metadata = new Dictionary<string, object>
            {
                ["Properties"] = domainEvent.Properties
            };

            await _activityLogger.LogAsync(
                activityType: "EntityCreated",
                description: description,
                entityId: domainEvent.EntityId,
                entityType: domainEvent.EntityType,
                metadata: metadata,
                severity: ActivitySeverity.Info,
                isPublic: false,
                tags: new[] { "EntityChange", domainEvent.EntityType, "Created" });
        }
        catch
        {
            // Silently fail - logging should not break the application
        }
    }

    public async Task HandleAsync(EntityUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var description = $"Entity {domainEvent.EntityType} with ID {domainEvent.EntityId} was updated";
            
            // Convert PropertyChange dictionary to a serializable format
            var changedProperties = domainEvent.ChangedProperties.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    OldValue = kvp.Value.OldValue,
                    NewValue = kvp.Value.NewValue
                });

            var metadata = new Dictionary<string, object>
            {
                ["ChangedProperties"] = changedProperties
            };

            await _activityLogger.LogAsync(
                activityType: "EntityUpdated",
                description: description,
                entityId: domainEvent.EntityId,
                entityType: domainEvent.EntityType,
                metadata: metadata,
                severity: ActivitySeverity.Info,
                isPublic: false,
                tags: new[] { "EntityChange", domainEvent.EntityType, "Updated" });
        }
        catch
        {
            // Silently fail - logging should not break the application
        }
    }

    public async Task HandleAsync(EntityDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var description = $"Entity {domainEvent.EntityType} with ID {domainEvent.EntityId} was deleted";
            
            var metadata = new Dictionary<string, object>
            {
                ["Properties"] = domainEvent.Properties
            };

            await _activityLogger.LogAsync(
                activityType: "EntityDeleted",
                description: description,
                entityId: domainEvent.EntityId,
                entityType: domainEvent.EntityType,
                metadata: metadata,
                severity: ActivitySeverity.Medium,
                isPublic: false,
                tags: new[] { "EntityChange", domainEvent.EntityType, "Deleted" });
        }
        catch
        {
            // Silently fail - logging should not break the application
        }
    }
}
*/
