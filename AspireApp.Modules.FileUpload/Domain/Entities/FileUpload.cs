using AspireApp.Domain.Shared.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;

namespace AspireApp.Modules.FileUpload.Domain.Entities;

/// <summary>
/// Represents an uploaded file in the system
/// </summary>
public class FileUpload : BaseEntity
{
    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Content type (MIME type) of the file
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// File extension (e.g., .jpg, .png, .pdf)
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Type/category of the file (Image, Document, Video, Audio, Other)
    /// </summary>
    public FileType FileType { get; set; }

    /// <summary>
    /// Storage type used to store this file (FileSystem, Database, R2)
    /// </summary>
    public FileStorageType StorageType { get; set; }

    /// <summary>
    /// Storage path or identifier (file path for FileSystem, R2 object key for R2, null for Database)
    /// </summary>
    public string? StoragePath { get; set; }

    /// <summary>
    /// File content stored in database (only used when StorageType is Database)
    /// </summary>
    public byte[]? FileContent { get; set; }

    /// <summary>
    /// ID of the user who uploaded the file
    /// </summary>
    public Guid? UploadedBy { get; set; }

    /// <summary>
    /// Optional description or notes about the file
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional tags for categorizing files
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// MD5 hash of the file content for integrity verification
    /// </summary>
    public string? Hash { get; set; }

    protected FileUpload() { }

    public FileUpload(
        string fileName,
        string contentType,
        long fileSize,
        string extension,
        FileType fileType,
        FileStorageType storageType,
        Guid? uploadedBy = null,
        string? description = null,
        string? tags = null,
        string? hash = null)
    {
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        Extension = extension;
        FileType = fileType;
        StorageType = storageType;
        UploadedBy = uploadedBy;
        Description = description;
        Tags = tags;
        Hash = hash;
    }
}

