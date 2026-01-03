using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.Domain.Shared.Helpers;

/// <summary>
/// Helper class for configuring entity table names with prefixes
/// </summary>
public static class EntityConfigurationHelper
{
    /// <summary>
    /// Default table prefix for all entities
    /// </summary>
    public const string DefaultTablePrefix = "App_";

    /// <summary>
    /// Configures entity table name with a prefix
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="builder">The entity type builder</param>
    /// <param name="tableName">The base table name (without prefix)</param>
    /// <param name="prefix">Optional custom prefix. If not provided, uses DefaultTablePrefix</param>
    public static EntityTypeBuilder<TEntity> ConfigureTableName<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string tableName,
        string? prefix = null) where TEntity : class
    {
        var tablePrefix = prefix ?? DefaultTablePrefix;
        return builder.ToTable($"{tablePrefix}{tableName}");
    }

    /// <summary>
    /// Applies table name prefix to all entities in the model
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    /// <param name="prefix">Optional custom prefix. If not provided, uses DefaultTablePrefix</param>
    public static void ApplyTablePrefixToAllEntities(
        this ModelBuilder modelBuilder,
        string? prefix = null)
    {
        var tablePrefix = prefix ?? DefaultTablePrefix;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Get the table name using the relational model
            var tableName = entityType.GetTableName();
            
            // Only add prefix if table has a name and doesn't already have the prefix
            if (!string.IsNullOrEmpty(tableName) && !tableName.StartsWith(tablePrefix))
            {
                entityType.SetTableName($"{tablePrefix}{tableName}");
            }
        }
    }

    /// <summary>
    /// Applies table name prefix to entities from a specific module
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    /// <param name="moduleNamespace">The module namespace (e.g., "AspireApp.Payment")</param>
    /// <param name="prefix">Optional custom prefix for the module</param>
    public static void ApplyModulePrefixToEntities(
        this ModelBuilder modelBuilder,
        string moduleNamespace,
        string prefix)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            
            // Check if entity belongs to the specified module
            if (clrType.Namespace != null && clrType.Namespace.StartsWith(moduleNamespace))
            {
                var tableName = entityType.GetTableName();
                
                // Only add prefix if table has a name and doesn't already have the prefix
                if (!string.IsNullOrEmpty(tableName) && !tableName.StartsWith(prefix))
                {
                    entityType.SetTableName($"{prefix}{tableName}");
                }
            }
        }
    }
}

