using AspireApp.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.PermissionId);
        builder.HasIndex(e => e.CreationTime);
    }
}

