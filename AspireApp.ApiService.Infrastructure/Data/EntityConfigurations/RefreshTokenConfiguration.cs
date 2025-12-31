using AspireApp.ApiService.Domain.Auth.Entities;
using AspireApp.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.Token).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => e.IsRevoked);
        builder.HasIndex(e => new { e.UserId, e.IsRevoked });
        builder.HasIndex(e => new { e.UserId, e.ExpiresAt });
        builder.HasIndex(e => e.CreationTime);

        // Properties
        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.IsRevoked)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

