using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace AspireApp.ApiService.Infrastructure.DomainEvents;

/// <summary>
/// Handles entity change domain events and logs them to activity log
/// Uses fire-and-forget pattern for better performance
/// </summary>
public class EntityChangeTrackingHandler :
    IDomainEventHandler<EntityCreatedEvent>,
    IDomainEventHandler<EntityUpdatedEvent>,
    IDomainEventHandler<EntityDeletedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EntityChangeTrackingHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task HandleAsync(EntityCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Fire-and-forget logging for better performance
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var activityLogger = scope.ServiceProvider.GetRequiredService<IActivityLogger>();

                // Special handling for relationship entities (junction tables) to provide user-friendly messages
                var entityType = GetEntityType(domainEvent.EntityType);
                if (entityType != null && IsRelationshipEntity(entityType))
                {
                    await HandleRelationshipEntityCreatedAsync(domainEvent, entityType, scope.ServiceProvider, activityLogger, cancellationToken);
                    return;
                }

                var metadata = new Dictionary<string, object>
                {
                    ["EntityType"] = domainEvent.EntityType,
                    ["EntityId"] = domainEvent.EntityId.ToString(),
                    ["State"] = "Added",
                    ["Properties"] = domainEvent.Properties
                };

                var description = $"Created {domainEvent.EntityType} (ID: {domainEvent.EntityId})";

                await activityLogger.LogAsync(
                    activityType: "EntityCreated",
                    description: description,
                    entityId: domainEvent.EntityId,
                    entityType: domainEvent.EntityType,
                    metadata: metadata,
                    severity: ActivitySeverity.Info,
                    isPublic: true,
                    "entity", "change", domainEvent.EntityType.ToLowerInvariant());
            }
            catch
            {
                // Silently fail - don't let logging break the operation
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the entity type from the type name string
    /// </summary>
    private static Type? GetEntityType(string typeName)
    {
        var domainAssembly = typeof(BaseEntity).Assembly;
        return domainAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == typeName && t.IsSubclassOf(typeof(BaseEntity)));
    }

    /// <summary>
    /// Determines if an entity is a relationship/junction entity (e.g., UserPermission, RolePermission, UserRole)
    /// Relationship entities typically have two ID properties ending with "Id" (e.g., UserId, PermissionId)
    /// </summary>
    private static bool IsRelationshipEntity(Type entityType)
    {
        var idProperties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.Name.EndsWith("Id") && p.PropertyType == typeof(Guid))
            .ToList();

        // Relationship entities have exactly 2 ID properties (besides the base Id property)
        return idProperties.Count == 2;
    }

    /// <summary>
    /// Gets the relationship information from a relationship entity
    /// Returns: (PrimaryEntityId, PrimaryEntityType, RelatedEntityId, RelatedEntityType)
    /// </summary>
    private static (Guid primaryId, string primaryType, Guid relatedId, string relatedType)? GetRelationshipInfo(
        BaseEntity entity,
        Type entityType)
    {
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var idProperties = properties
            .Where(p => p.Name.EndsWith("Id") && p.PropertyType == typeof(Guid) && p.Name != "Id")
            .ToList();

        if (idProperties.Count != 2)
            return null;

        var primaryProp = idProperties[0];
        var relatedProp = idProperties[1];

        var primaryId = (Guid)primaryProp.GetValue(entity)!;
        var relatedId = (Guid)relatedProp.GetValue(entity)!;

        // Determine entity types from property names (e.g., UserId -> User, PermissionId -> Permission)
        var primaryType = primaryProp.Name.Replace("Id", "");
        var relatedType = relatedProp.Name.Replace("Id", "");

        return (primaryId, primaryType, relatedId, relatedType);
    }

    /// <summary>
    /// Gets the display name for a related entity
    /// </summary>
    private async Task<string> GetRelatedEntityDisplayNameAsync(
        string entityTypeName,
        Guid entityId,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            var entityType = GetEntityType(entityTypeName);
            if (entityType == null)
                return $"{entityTypeName} (ID: {entityId})";

            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repositoryType = typeof(IRepository<>).MakeGenericType(entityType);
            var repository = serviceProvider.GetService(repositoryType);
            
            if (repository == null)
            {
                // Use reflection to call GetRepository<T> generic method
                var getRepositoryMethod = typeof(IUnitOfWork).GetMethod("GetRepository")!
                    .MakeGenericMethod(entityType);
                repository = getRepositoryMethod.Invoke(unitOfWork, null);
            }
            
            var getMethod = repositoryType.GetMethod("GetAsync", new[] { typeof(Guid), typeof(bool), typeof(CancellationToken) });
            if (getMethod == null)
                return $"{entityTypeName} (ID: {entityId})";

            var entityTask = (Task?)getMethod.Invoke(repository, new object[] { entityId, false, cancellationToken });
            if (entityTask == null)
                return $"{entityTypeName} (ID: {entityId})";

            await entityTask;
            var entity = ((dynamic)entityTask).Result;

            if (entity == null)
                return $"{entityTypeName} (ID: {entityId})";

            // Try to get a display name property (Name, GetFullPermissionName, etc.)
            var nameProperty = entityType.GetProperty("Name");
            if (nameProperty != null)
            {
                var name = nameProperty.GetValue(entity)?.ToString();
                if (!string.IsNullOrEmpty(name))
                    return name!;
            }

            // Try GetFullPermissionName method for Permission entities
            var fullNameMethod = entityType.GetMethod("GetFullPermissionName");
            if (fullNameMethod != null)
            {
                var fullName = fullNameMethod.Invoke(entity, null)?.ToString();
                if (!string.IsNullOrEmpty(fullName))
                    return fullName!;
            }

            return $"{entityTypeName} (ID: {entityId})";
        }
        catch
        {
            return $"{entityTypeName} (ID: {entityId})";
        }
    }

    private async Task HandleRelationshipEntityCreatedAsync(
        EntityCreatedEvent domainEvent,
        Type entityType,
        IServiceProvider serviceProvider,
        IActivityLogger activityLogger,
        CancellationToken cancellationToken)
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var getRepositoryMethod = typeof(IUnitOfWork).GetMethod("GetRepository")!
            .MakeGenericMethod(entityType);
        var repository = getRepositoryMethod.Invoke(unitOfWork, null);

        // Fetch the relationship entity
        var getMethod = typeof(IRepository<>).MakeGenericType(entityType)
            .GetMethod("GetAsync", new[] { typeof(Guid), typeof(bool), typeof(CancellationToken) });
        
        if (getMethod == null)
        {
            await LogDefaultEntityCreated(domainEvent, activityLogger);
            return;
        }

        var entityTask = (Task?)getMethod.Invoke(repository, new object[] { domainEvent.EntityId, false, cancellationToken });
        if (entityTask == null)
        {
            await LogDefaultEntityCreated(domainEvent, activityLogger);
            return;
        }

        await entityTask;
        var entity = ((dynamic)entityTask).Result as BaseEntity;

        if (entity == null)
        {
            await LogDefaultEntityCreated(domainEvent, activityLogger);
            return;
        }

        var relationshipInfo = GetRelationshipInfo(entity, entityType);
        if (relationshipInfo == null)
        {
            await LogDefaultEntityCreated(domainEvent, activityLogger);
            return;
        }

        var (primaryId, primaryType, relatedId, relatedType) = relationshipInfo.Value;

        // Get display names for both entities
        var relatedEntityName = await GetRelatedEntityDisplayNameAsync(relatedType, relatedId, serviceProvider, cancellationToken);

        var description = $"{relatedType} '{relatedEntityName}' was assigned to {primaryType} (ID: {primaryId})";

        var metadata = new Dictionary<string, object>
        {
            ["EntityType"] = domainEvent.EntityType,
            ["EntityId"] = domainEvent.EntityId.ToString(),
            [$"{primaryType}Id"] = primaryId.ToString(),
            [$"{relatedType}Id"] = relatedId.ToString(),
            [$"{relatedType}Name"] = relatedEntityName,
            ["Action"] = "Assigned"
        };

        await activityLogger.LogAsync(
            activityType: $"{domainEvent.EntityType}Changed",
            description: description,
            entityId: primaryId, // Use primary entity ID for better grouping
            entityType: primaryType,
            metadata: metadata,
            severity: ActivitySeverity.Info,
            isPublic: true,
            primaryType.ToLowerInvariant(), relatedType.ToLowerInvariant(), "assignment");
    }

    private async Task LogDefaultEntityCreated(
        EntityCreatedEvent domainEvent,
        IActivityLogger activityLogger)
    {
        var metadata = new Dictionary<string, object>
        {
            ["EntityType"] = domainEvent.EntityType,
            ["EntityId"] = domainEvent.EntityId.ToString(),
            ["State"] = "Added",
            ["Properties"] = domainEvent.Properties
        };

        var description = $"Created {domainEvent.EntityType} (ID: {domainEvent.EntityId})";

        await activityLogger.LogAsync(
            activityType: "EntityCreated",
            description: description,
            entityId: domainEvent.EntityId,
            entityType: domainEvent.EntityType,
            metadata: metadata,
            severity: ActivitySeverity.Info,
            isPublic: true,
            "entity", "change", domainEvent.EntityType.ToLowerInvariant());
    }

    public Task HandleAsync(EntityUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Fire-and-forget logging for better performance
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var activityLogger = scope.ServiceProvider.GetRequiredService<IActivityLogger>();

                // Special handling for relationship entities (junction tables) to provide user-friendly messages
                var entityType = GetEntityType(domainEvent.EntityType);
                if (entityType != null && IsRelationshipEntity(entityType) && IsSoftDeleteChange(domainEvent.ChangedProperties))
                {
                    await HandleRelationshipEntityUpdatedAsync(domainEvent, entityType, scope.ServiceProvider, activityLogger, cancellationToken);
                    return;
                }

                var changedProperties = domainEvent.ChangedProperties.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new Dictionary<string, object?>
                    {
                        ["OldValue"] = FormatValue(kvp.Value.OldValue),
                        ["NewValue"] = FormatValue(kvp.Value.NewValue)
                    });

                var metadata = new Dictionary<string, object>
                {
                    ["EntityType"] = domainEvent.EntityType,
                    ["EntityId"] = domainEvent.EntityId.ToString(),
                    ["State"] = "Modified",
                    ["ChangedProperties"] = changedProperties
                };

                var description = $"Updated {domainEvent.EntityType} (ID: {domainEvent.EntityId})";

                await activityLogger.LogAsync(
                    activityType: "EntityUpdated",
                    description: description,
                    entityId: domainEvent.EntityId,
                    entityType: domainEvent.EntityType,
                    metadata: metadata,
                    severity: ActivitySeverity.Medium,
                    isPublic: true,
                    "entity", "change", domainEvent.EntityType.ToLowerInvariant());
            }
            catch
            {
                // Silently fail - don't let logging break the operation
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private static bool IsSoftDeleteChange(Dictionary<string, PropertyChange> changedProperties)
    {
        // Check if the only changes are IsDeleted and/or DeletionTime
        var changeKeys = changedProperties.Keys.ToHashSet();
        return changeKeys.Count <= 2 && 
               changeKeys.All(k => k == "IsDeleted" || k == "DeletionTime");
    }

    private async Task HandleRelationshipEntityUpdatedAsync(
        EntityUpdatedEvent domainEvent,
        Type entityType,
        IServiceProvider serviceProvider,
        IActivityLogger activityLogger,
        CancellationToken cancellationToken)
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var getRepositoryMethod = typeof(IUnitOfWork).GetMethod("GetRepository")!
            .MakeGenericMethod(entityType);
        var repository = getRepositoryMethod.Invoke(unitOfWork, null);

        // Fetch the relationship entity (including deleted ones)
        var getMethod = typeof(IRepository<>).MakeGenericType(entityType)
            .GetMethod("GetAsync", new[] { typeof(Guid), typeof(bool), typeof(CancellationToken) });
        
        if (getMethod == null)
        {
            await LogDefaultEntityUpdated(domainEvent, activityLogger);
            return;
        }

        var entityTask = (Task?)getMethod.Invoke(repository, new object[] { domainEvent.EntityId, true, cancellationToken });
        if (entityTask == null)
        {
            await LogDefaultEntityUpdated(domainEvent, activityLogger);
            return;
        }

        await entityTask;
        var entity = ((dynamic)entityTask).Result as BaseEntity;

        if (entity == null)
        {
            await LogDefaultEntityUpdated(domainEvent, activityLogger);
            return;
        }

        var relationshipInfo = GetRelationshipInfo(entity, entityType);
        if (relationshipInfo == null)
        {
            await LogDefaultEntityUpdated(domainEvent, activityLogger);
            return;
        }

        var (primaryId, primaryType, relatedId, relatedType) = relationshipInfo.Value;

        // Get display name for the related entity
        var relatedEntityName = await GetRelatedEntityDisplayNameAsync(relatedType, relatedId, serviceProvider, cancellationToken);

        // Determine if this is a removal or addition based on IsDeleted change
        var isDeletedChange = domainEvent.ChangedProperties.TryGetValue("IsDeleted", out var isDeletedProp);
        var isRemoved = isDeletedChange && 
                       bool.TryParse(isDeletedProp?.NewValue?.ToString(), out var newIsDeleted) && 
                       newIsDeleted;

        var action = isRemoved ? "removed from" : "assigned to";
        var description = $"{relatedType} '{relatedEntityName}' was {action} {primaryType} (ID: {primaryId})";

        var metadata = new Dictionary<string, object>
        {
            ["EntityType"] = domainEvent.EntityType,
            ["EntityId"] = domainEvent.EntityId.ToString(),
            [$"{primaryType}Id"] = primaryId.ToString(),
            [$"{relatedType}Id"] = relatedId.ToString(),
            [$"{relatedType}Name"] = relatedEntityName,
            ["Action"] = isRemoved ? "Removed" : "Assigned"
        };

        await activityLogger.LogAsync(
            activityType: $"{domainEvent.EntityType}Changed",
            description: description,
            entityId: primaryId, // Use primary entity ID for better grouping
            entityType: primaryType,
            metadata: metadata,
            severity: ActivitySeverity.Medium,
            isPublic: true,
            primaryType.ToLowerInvariant(), relatedType.ToLowerInvariant(), "assignment");
    }

    private async Task LogDefaultEntityUpdated(
        EntityUpdatedEvent domainEvent,
        IActivityLogger activityLogger)
    {
        var changedProperties = domainEvent.ChangedProperties.ToDictionary(
            kvp => kvp.Key,
            kvp => new Dictionary<string, object?>
            {
                ["OldValue"] = FormatValue(kvp.Value.OldValue),
                ["NewValue"] = FormatValue(kvp.Value.NewValue)
            });

        var metadata = new Dictionary<string, object>
        {
            ["EntityType"] = domainEvent.EntityType,
            ["EntityId"] = domainEvent.EntityId.ToString(),
            ["State"] = "Modified",
            ["ChangedProperties"] = changedProperties
        };

        var description = $"Updated {domainEvent.EntityType} (ID: {domainEvent.EntityId})";

        await activityLogger.LogAsync(
            activityType: "EntityUpdated",
            description: description,
            entityId: domainEvent.EntityId,
            entityType: domainEvent.EntityType,
            metadata: metadata,
            severity: ActivitySeverity.Medium,
            isPublic: true,
            "entity", "change", domainEvent.EntityType.ToLowerInvariant());
    }

    public Task HandleAsync(EntityDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Fire-and-forget logging for better performance
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var activityLogger = scope.ServiceProvider.GetRequiredService<IActivityLogger>();

                var metadata = new Dictionary<string, object>
                {
                    ["EntityType"] = domainEvent.EntityType,
                    ["EntityId"] = domainEvent.EntityId.ToString(),
                    ["State"] = "Deleted",
                    ["Properties"] = domainEvent.Properties
                };

                var description = $"Deleted {domainEvent.EntityType} (ID: {domainEvent.EntityId})";

                await activityLogger.LogAsync(
                    activityType: "EntityDeleted",
                    description: description,
                    entityId: domainEvent.EntityId,
                    entityType: domainEvent.EntityType,
                    metadata: metadata,
                    severity: ActivitySeverity.High,
                    isPublic: true,
                    "entity", "change", domainEvent.EntityType.ToLowerInvariant());
            }
            catch
            {
                // Silently fail - don't let logging break the operation
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private static string? FormatValue(object? value)
    {
        if (value == null)
            return null;

        if (value is DateTime dateTime)
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");

        if (value is Guid guid)
            return guid.ToString();

        if (value is bool boolValue)
            return boolValue.ToString().ToLowerInvariant();

        if (value.GetType().IsClass && value.GetType() != typeof(string))
        {
            try
            {
                return JsonSerializer.Serialize(value);
            }
            catch
            {
                return value.ToString();
            }
        }

        return value.ToString();
    }
}

