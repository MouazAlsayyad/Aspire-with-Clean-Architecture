using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.Twilio.Infrastructure.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.RecipientPhoneNumber);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Channel);
        builder.HasIndex(e => e.MessageSid);
        builder.HasIndex(e => e.CreationTime);
        builder.HasIndex(e => new { e.RecipientPhoneNumber, e.Status });
        builder.HasIndex(e => new { e.RecipientPhoneNumber, e.Channel });

        // Properties
        builder.Property(e => e.RecipientPhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.MessageBody)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(e => e.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.MessageSid)
            .HasMaxLength(100);

        builder.Property(e => e.FailureReason)
            .HasMaxLength(500);

        builder.Property(e => e.TemplateId)
            .HasMaxLength(100);

        builder.Property(e => e.TemplateVariables)
            .HasMaxLength(2000);
    }
}

