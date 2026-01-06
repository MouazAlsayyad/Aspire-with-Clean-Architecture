using AspireApp.Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.Payment.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for PaymentTransaction entity
/// </summary>
public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        // Configure Money value object
        builder.ComplexProperty(t => t.Amount, money =>
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

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Response)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.PaymentId);
        builder.HasIndex(t => t.TransactionDate);
    }
}

