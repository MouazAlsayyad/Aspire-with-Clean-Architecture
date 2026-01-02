using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;

namespace AspireApp.ApiService.Infrastructure.Services.FileStorage;

/// <summary>
/// Factory for creating file storage strategies based on storage type.
/// Uses dependency injection to resolve the appropriate strategy.
/// </summary>
public class FileStorageStrategyFactory : IFileStorageStrategyFactory
{
    private readonly Dictionary<FileStorageType, IFileStorageStrategy> _strategies;

    public FileStorageStrategyFactory(IEnumerable<IFileStorageStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.StorageType, s => s);
    }

    /// <summary>
    /// Gets the storage strategy for the specified storage type
    /// </summary>
    public IFileStorageStrategy GetStrategy(FileStorageType storageType)
    {
        if (!_strategies.TryGetValue(storageType, out var strategy))
        {
            throw new NotSupportedException($"Storage type {storageType} is not supported or not registered.");
        }

        return strategy;
    }
}

