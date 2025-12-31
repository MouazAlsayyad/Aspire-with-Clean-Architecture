using AspireApp.ApiService.Domain.FileUploads.Enums;

namespace AspireApp.ApiService.Application.FileUpload.DTOs;

/// <summary>
/// DTO representing a file upload
/// </summary>
public record FileUploadDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Extension { get; init; } = string.Empty;
    public FileType FileType { get; init; }
    public FileStorageType StorageType { get; init; }
    public string? StoragePath { get; init; }
    public Guid? UploadedBy { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public string? Hash { get; init; }
    public DateTime CreationTime { get; init; }
}

