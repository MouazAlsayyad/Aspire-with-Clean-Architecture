using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using FileUploadEntity = AspireApp.Modules.FileUpload.Domain.Entities.FileUpload;

namespace AspireApp.Modules.FileUpload.Domain.Interfaces;

/// <summary>
/// Repository interface for FileUpload entity with specific queries
/// </summary>
public interface IFileUploadRepository : IRepository<FileUploadEntity>
{
    /// <summary>
    /// Gets files uploaded by a specific user
    /// </summary>
    Task<List<FileUploadEntity>> GetByUploadedByAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by storage type
    /// </summary>
    Task<List<FileUploadEntity>> GetByStorageTypeAsync(FileStorageType storageType, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by file type
    /// </summary>
    Task<List<FileUploadEntity>> GetByFileTypeAsync(FileType fileType, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by hash (for duplicate detection)
    /// </summary>
    Task<FileUploadEntity?> GetByHashAsync(string hash, bool includeDeleted = false, CancellationToken cancellationToken = default);
}

