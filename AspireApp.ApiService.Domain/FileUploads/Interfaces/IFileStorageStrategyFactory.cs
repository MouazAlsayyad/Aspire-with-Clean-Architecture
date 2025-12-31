using AspireApp.ApiService.Domain.FileUploads.Enums;

namespace AspireApp.ApiService.Domain.FileUploads.Interfaces;

/// <summary>
/// Factory interface for creating file storage strategies.
/// This allows the Application layer to use storage strategies without depending on Infrastructure.
/// </summary>
public interface IFileStorageStrategyFactory
{
    /// <summary>
    /// Gets the storage strategy for the specified storage type
    /// </summary>
    IFileStorageStrategy GetStrategy(FileStorageType storageType);
}

