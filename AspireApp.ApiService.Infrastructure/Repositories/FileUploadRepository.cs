using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Repositories;

public class FileUploadRepository : Repository<FileUpload>, IFileUploadRepository
{
    public FileUploadRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<FileUpload>> GetByUploadedByAsync(
        Guid userId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FileUploads
            .Where(f => f.UploadedBy == userId)
            .AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FileUpload>> GetByStorageTypeAsync(
        FileStorageType storageType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FileUploads
            .Where(f => f.StorageType == storageType)
            .AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FileUpload>> GetByFileTypeAsync(
        FileType fileType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FileUploads
            .Where(f => f.FileType == fileType)
            .AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<FileUpload?> GetByHashAsync(
        string hash,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FileUploads
            .Where(f => f.Hash == hash)
            .AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}

