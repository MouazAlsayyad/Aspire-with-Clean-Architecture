using AspireApp.ApiService.Domain.Enums;

namespace AspireApp.ApiService.Application.DTOs.FileUpload;

/// <summary>
/// Request DTO for uploading a file
/// </summary>
public record UploadFileRequest
{
    /// <summary>
    /// Storage type to use (FileSystem, Database, R2)
    /// </summary>
    public FileStorageType StorageType { get; init; } = FileStorageType.FileSystem;

    /// <summary>
    /// Optional description for the file
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional tags for categorizing the file
    /// </summary>
    public string? Tags { get; init; }
}

