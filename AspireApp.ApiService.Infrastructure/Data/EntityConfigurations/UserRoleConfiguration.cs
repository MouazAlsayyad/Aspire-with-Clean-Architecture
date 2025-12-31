using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspireApp.ApiService.Infrastructure.Data.EntityConfigurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => e.CreationTime);
    }
}

