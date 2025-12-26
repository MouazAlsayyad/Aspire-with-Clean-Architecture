using AspireApp.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => e.PermissionId);
        builder.HasIndex(e => e.CreationTime);
    }
}

