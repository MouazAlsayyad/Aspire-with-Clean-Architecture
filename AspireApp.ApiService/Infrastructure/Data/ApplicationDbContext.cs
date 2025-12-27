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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft delete - automatically applies to all entities inheriting from BaseEntity
        // Use IgnoreQueryFilters() to include deleted entities when needed
        ApplyGlobalQueryFilters(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasConversion(
                v => $"{v.Hash}:{v.Salt}",
                v => ParsePasswordHash(v)
            );

            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure navigation to use backing field for proper change tracking
            entity.Navigation(e => e.UserRoles)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired();

            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserRole configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);

            entity.HasMany(e => e.RolePermissions)
                .WithOne(e => e.Permission)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RolePermission configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });
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

    private static Domain.ValueObjects.PasswordHash ParsePasswordHash(string value)
    {
        var parts = value.Split(':');
        if (parts.Length != 2)
            throw new InvalidOperationException("Invalid password hash format");

        return new Domain.ValueObjects.PasswordHash(parts[0], parts[1]);
    }
}

