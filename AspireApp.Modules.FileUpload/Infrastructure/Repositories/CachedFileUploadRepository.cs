using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AspireApp.Modules.FileUpload.Domain.Enums;
using Microsoft.Extensions.Logging;
using FileUploadEntity = AspireApp.Modules.FileUpload.Domain.Entities.FileUpload;

namespace AspireApp.Modules.FileUpload.Infrastructure.Repositories;

public class CachedFileUploadRepository : CachedRepository<FileUploadEntity>, IFileUploadRepository
{
    private readonly IFileUploadRepository _fileUploadRepository;

    public CachedFileUploadRepository(
        IFileUploadRepository decorated,
        ICacheService cacheService,
        ILogger<CachedFileUploadRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _fileUploadRepository = decorated;
    }

    public async Task<List<FileUploadEntity>> GetByUploadedByAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (includeDeleted)
        {
            return await _fileUploadRepository.GetByUploadedByAsync(userId, includeDeleted, cancellationToken);
        }

        string key = $"fileupload:uploadedby:{userId}";
        
        var result = await _cacheService.GetOrSetAsync(
            key,
            ct => _fileUploadRepository.GetByUploadedByAsync(userId, includeDeleted, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken);

        return result ?? new List<FileUploadEntity>();
    }

    public async Task<List<FileUploadEntity>> GetByStorageTypeAsync(FileStorageType storageType, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        // Don't cache by storage type usually
        return await _fileUploadRepository.GetByStorageTypeAsync(storageType, includeDeleted, cancellationToken);
    }

    public async Task<List<FileUploadEntity>> GetByFileTypeAsync(FileType fileType, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return await _fileUploadRepository.GetByFileTypeAsync(fileType, includeDeleted, cancellationToken);
    }

    public async Task<FileUploadEntity?> GetByHashAsync(string hash, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        string key = $"fileupload:hash:{hash}";
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _fileUploadRepository.GetByHashAsync(hash, includeDeleted, ct),
            TimeSpanConstants.TwentyFourHours,
            cancellationToken);
    }
}
