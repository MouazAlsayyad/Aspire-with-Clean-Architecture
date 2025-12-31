using AspireApp.ApiService.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace AspireApp.ApiService.Application.DTOs.FileUpload;

/// <summary>
/// Form DTO for file upload endpoint
/// </summary>
public class UploadFileFormDto
{
    /// <summary>
    /// The file to upload
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    /// Storage type to use (FileSystem, Database, R2)
    /// </summary>
    public FileStorageType? StorageType { get; set; }

    /// <summary>
    /// Optional description for the file
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional tags for categorizing the file
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// If "true" or "1", processes the file upload in the background queue for faster response times.
    /// When enabled, the endpoint returns immediately while the file is processed asynchronously.
    /// Accepts: "true", "false", "1", "0", or any case variation.
    /// </summary>
    public string? UseBackgroundQueue { get; set; }
}

