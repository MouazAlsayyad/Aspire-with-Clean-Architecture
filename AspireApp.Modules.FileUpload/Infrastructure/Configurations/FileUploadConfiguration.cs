using AspireApp.Modules.FileUpload.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileUploadEntity = AspireApp.Modules.FileUpload.Domain.Entities.FileUpload;

namespace AspireApp.Modules.FileUpload.Infrastructure.Configurations;

public class FileUploadConfiguration : IEntityTypeConfiguration<FileUploadEntity>
{
    public void Configure(EntityTypeBuilder<FileUploadEntity> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.FileName);
        builder.HasIndex(e => e.UploadedBy);
        builder.HasIndex(e => e.StorageType);
        builder.HasIndex(e => e.FileType);
        builder.HasIndex(e => e.Hash);
        builder.HasIndex(e => e.CreationTime);
        builder.HasIndex(e => new { e.UploadedBy, e.CreationTime });
        builder.HasIndex(e => new { e.StorageType, e.FileType });

        // Properties
        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FileSize)
            .IsRequired();

        builder.Property(e => e.Extension)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.FileType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.StorageType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.StoragePath)
            .HasMaxLength(1000);

        builder.Property(e => e.FileContent)
            .HasColumnType("varbinary(max)");

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Tags)
            .HasMaxLength(500);

        builder.Property(e => e.Hash)
            .HasMaxLength(64);
    }
}

