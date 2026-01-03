using AspireApp.FirebaseNotifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.FirebaseNotifications.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsRead);
        builder.HasIndex(e => e.CreationTime);
        builder.HasIndex(e => new { e.UserId, e.IsRead });
        builder.HasIndex(e => new { e.UserId, e.CreationTime });

        // Properties
        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.TitleAr)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.MessageAr)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.ActionUrl)
            .HasMaxLength(500);

        builder.Property(e => e.UserId)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

