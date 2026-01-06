using AspireApp.Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.Payment.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Payment entity
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.OrderNumber)
            .IsUnique();

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        // Configure Money value object
        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Amount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("Currency");
        });

        builder.Property(p => p.ExternalReference)
            .HasMaxLength(500);

        builder.HasIndex(p => p.ExternalReference);

        builder.Property(p => p.CustomerEmail)
            .HasMaxLength(500);

        builder.Property(p => p.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(p => p.Metadata)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasMany(p => p.Transactions)
            .WithOne(t => t.Payment)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreationTime);
    }
}

