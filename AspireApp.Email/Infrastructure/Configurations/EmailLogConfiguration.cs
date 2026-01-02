using AspireApp.Email.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.Email.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for EmailLog entity
/// </summary>
public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes for common queries
        builder.HasIndex(e => e.ToAddress);
        builder.HasIndex(e => e.EmailType);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreationTime);
        builder.HasIndex(e => e.SentAt);
        builder.HasIndex(e => new { e.Status, e.RetryCount });
        builder.HasIndex(e => new { e.EmailType, e.Status });
        builder.HasIndex(e => new { e.CreationTime, e.Status });

        // Properties
        builder.Property(e => e.EmailType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ToAddress)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FromAddress)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.MessageId)
            .HasMaxLength(100);

        builder.Property(e => e.BccAddresses)
            .HasMaxLength(1000);

        builder.Property(e => e.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.HasAttachments)
            .IsRequired()
            .HasDefaultValue(false);
    }
}

