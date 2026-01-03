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

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

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

