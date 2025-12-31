using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Services.FileStorage;

/// <summary>
/// File storage strategy that saves files to the database as binary data.
/// Note: This strategy returns a GUID as the storage path identifier.
/// The actual file content is stored in the FileUpload entity's FileContent property.
/// </summary>
public class DatabaseStorageStrategy : IFileStorageStrategy
{
    private readonly ILogger<DatabaseStorageStrategy> _logger;

    public FileStorageType StorageType => FileStorageType.Database;

    public DatabaseStorageStrategy(ILogger<DatabaseStorageStrategy> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadAsync(
        string fileName,
        string contentType,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        // For database storage, we return a GUID identifier.
        // The actual file content will be stored in the FileUpload entity's FileContent property
        // by the use case that calls this strategy.
        var identifier = Guid.NewGuid().ToString();
        _logger.LogInformation("File prepared for database storage: {FileName}, Identifier: {Identifier}", fileName, identifier);
        return Task.FromResult(identifier);
    }

    public Task<Stream> DownloadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        // This method should not be called directly for database storage.
        // The file content should be retrieved from the FileUpload entity's FileContent property.
        _logger.LogWarning("DownloadAsync called for database storage. File content should be retrieved from FileUpload entity.");
        throw new InvalidOperationException(
            "For database storage, file content should be retrieved from the FileUpload entity's FileContent property, not through DownloadAsync.");
    }

    public Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        // For database storage, deletion is handled by deleting the FileUpload entity.
        // This method is a no-op as the file content is stored in the entity itself.
        _logger.LogInformation("DeleteAsync called for database storage. File deletion should be handled by deleting the FileUpload entity.");
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        // For database storage, existence is checked by querying the FileUpload entity.
        // This method always returns false as we can't check existence without the repository.
        _logger.LogWarning("ExistsAsync called for database storage. Existence should be checked via FileUpload repository.");
        return Task.FromResult(false);
    }
}

