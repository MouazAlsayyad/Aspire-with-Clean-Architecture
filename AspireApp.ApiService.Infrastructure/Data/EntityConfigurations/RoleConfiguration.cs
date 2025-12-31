using AspireApp.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.CreationTime);
        builder.HasIndex(e => e.LastModificationTime);

        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Type)
            .IsRequired();

        // Relationships
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.Role)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.RolePermissions)
            .WithOne(e => e.Role)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure the navigation to use backing field for change detection
        builder.Navigation(e => e.RolePermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

