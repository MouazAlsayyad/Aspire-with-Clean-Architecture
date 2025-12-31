using AspireApp.ApiService.Domain.FileUploads.Enums;

namespace AspireApp.ApiService.Application.FileUpload.DTOs;

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

    /// <summary>
    /// If true, processes the file upload in the background queue for faster response times.
    /// When enabled, the endpoint returns immediately while the file is processed asynchronously.
    /// </summary>
    public bool UseBackgroundQueue { get; init; } = false;
}

