using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using FileUploadEntity = AspireApp.Modules.FileUpload.Domain.Entities.FileUpload;

namespace AspireApp.Modules.FileUpload.Domain.Interfaces;

/// <summary>
/// Interface for FileUpload domain service (Manager).
/// Handles file upload-related domain logic and business rules.
/// Domain services throw DomainException for business rule violations.
/// </summary>
public interface IFileUploadManager : IDomainService
{
    /// <summary>
    /// Validates a file according to domain rules.
    /// Throws DomainException if validation fails.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="contentType">The content type (MIME type)</param>
    /// <param name="fileSize">The file size in bytes</param>
    /// <param name="fileStream">The file stream for hash computation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the file extension, file type, and computed hash</returns>
    Task<(string Extension, FileType FileType, string? Hash)> ValidateAndProcessFileAsync(
        string fileName,
        string contentType,
        long fileSize,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a FileUpload entity with domain validation.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="contentType">The content type</param>
    /// <param name="fileSize">The file size</param>
    /// <param name="extension">The file extension</param>
    /// <param name="fileType">The file type</param>
    /// <param name="storageType">The storage type</param>
    /// <param name="uploadedBy">The user ID who uploaded the file</param>
    /// <param name="description">Optional description</param>
    /// <param name="tags">Optional tags</param>
    /// <param name="hash">Optional file hash</param>
    /// <param name="storagePath">Optional storage path (null for background processing)</param>
    /// <param name="fileContent">Optional file content (for Database storage)</param>
    /// <returns>The created FileUpload entity</returns>
    FileUploadEntity CreateFileUpload(
        string fileName,
        string contentType,
        long fileSize,
        string extension,
        FileType fileType,
        FileStorageType storageType,
        Guid? uploadedBy = null,
        string? description = null,
        string? tags = null,
        string? hash = null,
        string? storagePath = null,
        byte[]? fileContent = null);

    /// <summary>
    /// Reads file stream into memory buffer for background processing.
    /// </summary>
    /// <param name="fileStream">The file stream to read</param>
    /// <param name="fileSize">The expected file size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file bytes</returns>
    Task<byte[]> ReadFileIntoMemoryAsync(
        Stream fileStream,
        long fileSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes file hash for duplicate detection and integrity verification.
    /// </summary>
    /// <param name="fileBytes">The file bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The computed hash, or null if computation fails</returns>
    Task<string?> ComputeFileHashAsync(
        byte[] fileBytes,
        CancellationToken cancellationToken = default);
}

