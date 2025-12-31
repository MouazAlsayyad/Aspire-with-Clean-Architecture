using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.IsEmailConfirmed);
        builder.HasIndex(e => e.CreationTime);
        builder.HasIndex(e => e.LastModificationTime);

        // Properties
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PasswordHash)
            .HasConversion(
                v => $"{v.Hash}:{v.Salt}",
                v => ParsePasswordHash(v)
            );

        builder.Property(e => e.IsEmailConfirmed)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(e => e.FcmToken)
            .HasMaxLength(500);

        // Relationships - configure backing field access for proper change tracking
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the navigation to use backing field for change detection
        builder.Navigation(e => e.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.UserPermissions)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the navigation to use backing field for change detection
        builder.Navigation(e => e.UserPermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static PasswordHash ParsePasswordHash(string value)
    {
        var parts = value.Split(':');
        if (parts.Length != 2)
            throw new InvalidOperationException("Invalid password hash format");

        return new PasswordHash(parts[0], parts[1]);
    }
}

