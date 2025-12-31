using AspireApp.ApiService.Domain.FileUploads.Enums;

namespace AspireApp.ApiService.Domain.FileUploads.Interfaces;

/// <summary>
/// Strategy interface for file storage implementations.
/// This follows the Strategy pattern to allow different storage methods.
/// </summary>
public interface IFileStorageStrategy
{
    /// <summary>
    /// The storage type this strategy handles
    /// </summary>
    FileStorageType StorageType { get; }

    /// <summary>
    /// Uploads a file using this storage strategy
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="fileStream">Stream containing the file data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage path or identifier for the uploaded file</returns>
    Task<string> UploadAsync(
        string fileName,
        string contentType,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads/retrieves a file from storage
    /// </summary>
    /// <param name="storagePath">Storage path or identifier returned from UploadAsync</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream containing the file data</returns>
    Task<Stream> DownloadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="storagePath">Storage path or identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="storagePath">Storage path or identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}

