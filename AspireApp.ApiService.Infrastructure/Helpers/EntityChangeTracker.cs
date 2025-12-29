using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace AspireApp.ApiService.Infrastructure.Helpers;

/// <summary>
/// Tracks changes to entities and raises domain events (DDD-compliant approach)
/// </summary>
public class EntityChangeTracker
{
    /// <summary>
    /// Raises domain events for entity changes (DDD-compliant approach)
    /// </summary>
    public static void RaiseDomainEventsForChanges(ChangeTracker changeTracker)
    {
        var entries = changeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            var entityType = entity.GetType();
            var entityTypeName = entityType.Name;
            var entityId = entity.Id;

            // Skip ActivityLog entities to avoid recursive logging
            if (entityTypeName == "ActivityLog")
                continue;

            // Skip entities marked with ExcludeFromLogging attribute
            if (entityType.GetCustomAttribute<ExcludeFromLoggingAttribute>() != null)
                continue;

            if (entry.State == EntityState.Added)
            {
                var properties = GetEntityProperties(entry, entityType, includeMetadata: false);
                entity.AddDomainEvent(new EntityCreatedEvent(entityId, entityTypeName, properties));
            }
            else if (entry.State == EntityState.Modified)
            {
                var changedProperties = GetChangedProperties(entry, entityType);
                if (changedProperties.Any())
                {
                    entity.AddDomainEvent(new EntityUpdatedEvent(entityId, entityTypeName, changedProperties));
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                var properties = GetEntityProperties(entry, entityType, includeMetadata: true);
                entity.AddDomainEvent(new EntityDeletedEvent(entityId, entityTypeName, properties));
            }
        }
    }

    private static Dictionary<string, object?> GetEntityProperties(EntityEntry<BaseEntity> entry, Type entityType, bool includeMetadata)
    {
        var properties = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            // Skip metadata properties
            if (!includeMetadata && (propertyName == "Id" ||
                propertyName == "CreationTime" ||
                propertyName == "LastModificationTime" ||
                propertyName == "LastModifiedBy"))
                continue;

            // Skip properties/fields marked with ExcludeFromLogging attribute
            if (IsExcludedFromLogging(entityType, propertyName))
                continue;

            var value = entry.State == EntityState.Deleted 
                ? property.OriginalValue 
                : property.CurrentValue;

            properties[propertyName] = value;
        }

        return properties;
    }

    private static Dictionary<string, PropertyChange> GetChangedProperties(EntityEntry<BaseEntity> entry, Type entityType)
    {
        var changedProperties = new Dictionary<string, PropertyChange>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            // Skip metadata properties
            if (propertyName == "LastModificationTime" ||
                propertyName == "LastModifiedBy")
                continue;

            // Skip properties/fields marked with ExcludeFromLogging attribute
            if (IsExcludedFromLogging(entityType, propertyName))
                continue;

            if (property.IsModified)
            {
                var originalValue = property.OriginalValue;
                var currentValue = property.CurrentValue;

                // Skip if values are the same
                if (AreValuesEqual(originalValue, currentValue))
                    continue;

                changedProperties[propertyName] = new PropertyChange
                {
                    OldValue = originalValue,
                    NewValue = currentValue
                };
            }
        }

        return changedProperties;
    }

    /// <summary>
    /// Checks if a property or its backing field is marked with ExcludeFromLogging attribute
    /// </summary>
    private static bool IsExcludedFromLogging(Type entityType, string propertyName)
    {
        // Check the property itself
        var propertyInfo = entityType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (propertyInfo?.GetCustomAttribute<ExcludeFromLoggingAttribute>() != null)
            return true;

        // Check for a field with the same name (EF Core may use backing fields)
        var fieldInfo = entityType.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfo?.GetCustomAttribute<ExcludeFromLoggingAttribute>() != null)
            return true;

        // Check for backing field with underscore prefix (common convention, e.g., _userPermissions)
        var backingFieldName = $"_{propertyName}";
        var backingFieldInfo = entityType.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (backingFieldInfo?.GetCustomAttribute<ExcludeFromLoggingAttribute>() != null)
            return true;

        // Check for camelCase backing field (e.g., userPermissions -> _userPermissions)
        if (propertyName.Length > 0)
        {
            var camelCaseBackingField = $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
            var camelCaseFieldInfo = entityType.GetField(camelCaseBackingField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (camelCaseFieldInfo?.GetCustomAttribute<ExcludeFromLoggingAttribute>() != null)
                return true;
        }

        return false;
    }

    private static bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
            return true;
        if (value1 == null || value2 == null)
            return false;

        try
        {
            var json1 = System.Text.Json.JsonSerializer.Serialize(value1);
            var json2 = System.Text.Json.JsonSerializer.Serialize(value2);
            return json1 == json2;
        }
        catch
        {
            return value1.Equals(value2);
        }
    }
}
