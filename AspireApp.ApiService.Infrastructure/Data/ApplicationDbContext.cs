using AspireApp.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AspireApp.ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete - automatically applies to all entities inheriting from BaseEntity
        // Use IgnoreQueryFilters() to include deleted entities when needed
        ApplyGlobalQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Applies global query filter for soft delete to all entities that inherit from BaseEntity
    /// </summary>
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Get all entity types that inherit from BaseEntity
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
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
    }

}

