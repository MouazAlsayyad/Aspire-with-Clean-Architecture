using AspireApp.Twilio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Twilios.Configurations;

public class OtpConfiguration : IEntityTypeConfiguration<Otp>
{
    public void Configure(EntityTypeBuilder<Otp> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.PhoneNumber);
        builder.HasIndex(e => e.Code);
        builder.HasIndex(e => e.IsUsed);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.PhoneNumber, e.IsUsed, e.ExpiresAt });

        // Properties
        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ExpiresAt)
            .IsRequired();
    }
}

