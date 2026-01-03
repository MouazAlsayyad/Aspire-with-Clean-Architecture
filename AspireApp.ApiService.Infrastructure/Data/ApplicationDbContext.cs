using AspireApp.ApiService.Domain.Authentication.Entities;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.Modules.ActivityLogs.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace AspireApp.ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<FileUpload> FileUploads => Set<FileUpload>();
    // Note: Notification, EmailLog, Message and Otp DbSets are managed by their modules to avoid circular dependency

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Apply configurations from module assemblies dynamically
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        if (notificationsAssembly != null)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(notificationsAssembly);
        }
        
        var emailAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailAssembly != null)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(emailAssembly);
        }
        
        var twilioAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        if (twilioAssembly != null)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(twilioAssembly);
        }

        // Global query filter for soft delete - automatically applies to all entities inheriting from BaseEntity
        // Use IgnoreQueryFilters() to include deleted entities when needed
        ApplyGlobalQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Applies global query filter for soft delete to all entities that inherit from BaseEntity
    /// Excludes ActivityLog since logs are permanent records
    /// </summary>
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Get all entity types that inherit from BaseEntity, excluding ActivityLog
        var activityLogType = typeof(ActivityLog);
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType) && e.ClrType != activityLogType)
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // Create a parameter expression for the entity (e => ...)
            var parameter = Expression.Parameter(entityType.ClrType, "e");

            // Create property access expression (e.IsDeleted)
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));

            // Create negation expression (!e.IsDeleted)
            var filterExpression = Expression.Lambda(
                Expression.Not(property),
                parameter
            );

            // Apply the query filter
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filterExpression);
        }

        // Note: ActivityLog configuration is handled by ActivityLogConfiguration in the ActivityLogs module
        // ActivityLog is excluded from soft delete filter (handled in ApplyGlobalQueryFilters above)
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Ensure new entities added to navigation property collections are tracked
        EnsureNavigationPropertiesAreTracked();

        // Raise domain events for entity changes (DDD-compliant)
        Helpers.EntityChangeTracker.RaiseDomainEventsForChanges(ChangeTracker);

        // Collect domain events from entities before saving
        var domainEvents = ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        if (domainEvents.Any() && _domainEventDispatcher != null)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        // Clear domain events after dispatching
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    /// <summary>
    /// Ensures that new entities added to navigation property collections are tracked by EF Core.
    /// This is a general solution that works for all entities following ABP Framework patterns.
    /// </summary>
    private void EnsureNavigationPropertiesAreTracked()
    {
        // Get all tracked entities that are Modified (have changes)
        var modifiedEntries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Unchanged)
            .ToList();

        foreach (var entry in modifiedEntries)
        {
            // Get all collection navigation properties
            var collectionNavigations = entry.Navigations
                .Where(n => n.Metadata.IsCollection)
                .ToList();

            foreach (var navigation in collectionNavigations)
            {
                // Check if the collection is loaded
                if (!navigation.IsLoaded)
                {
                    continue; // Skip unloaded collections
                }

                // Get the collection value
                var collectionValue = navigation.CurrentValue;
                if (collectionValue == null)
                {
                    continue;
                }

                // Use reflection to get the actual collection items
                var itemType = navigation.Metadata.TargetEntityType.ClrType;
                if (!typeof(BaseEntity).IsAssignableFrom(itemType))
                {
                    continue; // Skip if items are not BaseEntity
                }

                // Convert to enumerable and check each item
                var items = ((System.Collections.IEnumerable)collectionValue).Cast<BaseEntity>().ToList();

                foreach (var item in items)
                {
                    var itemEntry = Entry(item);
                    if (itemEntry.State == EntityState.Detached)
                    {
                        // New entity in collection - add it to the context so EF Core tracks it
                        // Use reflection to get the appropriate DbSet and add the entity
                        var dbSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes);
                        var genericDbSetMethod = dbSetMethod?.MakeGenericMethod(itemType);
                        var dbSet = genericDbSetMethod?.Invoke(this, null);

                        if (dbSet != null)
                        {
                            var addMethod = dbSet.GetType().GetMethod(nameof(DbSet<BaseEntity>.Add));
                            if (addMethod != null)
                            {
                                addMethod.Invoke(dbSet, new object[] { item });
                            }
                        }
                    }
                }
            }
        }
    }

}

