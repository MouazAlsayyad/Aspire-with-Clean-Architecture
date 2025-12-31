using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.FileUploads.Entities;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Domain.FileUploads.Interfaces;

/// <summary>
/// Repository interface for FileUpload entity with specific queries
/// </summary>
public interface IFileUploadRepository : IRepository<FileUpload>
{
    /// <summary>
    /// Gets files uploaded by a specific user
    /// </summary>
    Task<List<FileUpload>> GetByUploadedByAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by storage type
    /// </summary>
    Task<List<FileUpload>> GetByStorageTypeAsync(FileStorageType storageType, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by file type
    /// </summary>
    Task<List<FileUpload>> GetByFileTypeAsync(FileType fileType, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by hash (for duplicate detection)
    /// </summary>
    Task<FileUpload?> GetByHashAsync(string hash, bool includeDeleted = false, CancellationToken cancellationToken = default);
}

