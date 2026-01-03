using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(x => x.Id);

        // Indexes for performance
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ActivityType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.CreationTime);
        builder.HasIndex(x => x.Severity);
        builder.HasIndex(x => new { x.UserId, x.CreationTime });
        builder.HasIndex(x => new { x.EntityId, x.EntityType });

        // Properties
        builder.Property(x => x.ActivityType)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.DescriptionTemplate)
            .IsRequired();

        builder.Property(x => x.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsPublic)
            .IsRequired()
            .HasDefaultValue(true);
    }
}

