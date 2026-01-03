using AspireApp.Domain.Shared.Common;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using FileUploadEntity = AspireApp.Modules.FileUpload.Domain.Entities.FileUpload;

namespace AspireApp.Modules.FileUpload.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for FileUpload entity
/// Note: This repository is registered dynamically from Program.cs where ApplicationDbContext is available
/// </summary>
public class FileUploadRepository : IFileUploadRepository
{
    private readonly DbContext _context;
    private readonly DbSet<FileUploadEntity> _dbSet;

    public FileUploadRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<FileUploadEntity>();
    }

    #region IFileUploadRepository Methods

    public async Task<List<FileUploadEntity>> GetByUploadedByAsync(
        Guid userId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(f => f.UploadedBy == userId);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FileUploadEntity>> GetByStorageTypeAsync(
        FileStorageType storageType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(f => f.StorageType == storageType);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FileUploadEntity>> GetByFileTypeAsync(
        FileType fileType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(f => f.FileType == fileType);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<FileUploadEntity?> GetByHashAsync(
        string hash,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(f => f.Hash == hash);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region IRepository<FileUpload> Methods

    public async Task<FileUploadEntity?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(e => e.Id == id);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FileUploadEntity?> FindAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return await GetAsync(id, includeDeleted, cancellationToken);
    }

    public async Task<List<FileUploadEntity>> GetListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FileUploadEntity>> GetListAsync(Expression<Func<FileUploadEntity, bool>> predicate, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(predicate);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IQueryable<FileUploadEntity>> GetQueryableAsync(bool includeDeleted = false)
    {
        var query = _dbSet.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await Task.FromResult(query);
    }

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(Expression<Func<FileUploadEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<FileUploadEntity> InsertAsync(FileUploadEntity entity, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    public async Task<FileUploadEntity> InsertAsync(FileUploadEntity entity, CancellationToken cancellationToken = default)
    {
        return await InsertAsync(entity, true, cancellationToken);
    }

    public async Task InsertManyAsync(IEnumerable<FileUploadEntity> entities, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<FileUploadEntity> UpdateAsync(FileUploadEntity entity, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    public async Task<FileUploadEntity> UpdateAsync(FileUploadEntity entity, CancellationToken cancellationToken = default)
    {
        return await UpdateAsync(entity, true, cancellationToken);
    }

    public async Task UpdateManyAsync(IEnumerable<FileUploadEntity> entities, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(id, includeDeleted: true, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, hardDelete, cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(id, false, cancellationToken);
    }

    public async Task DeleteAsync(FileUploadEntity entity, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        if (hardDelete)
        {
            _dbSet.Remove(entity);
        }
        else
        {
            entity.Delete();
            _dbSet.Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FileUploadEntity entity, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(entity, false, cancellationToken);
    }

    public async Task DeleteManyAsync(IEnumerable<FileUploadEntity> entities, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        if (hardDelete)
        {
            _dbSet.RemoveRange(entities);
        }
        else
        {
            foreach (var entity in entities)
            {
                entity.Delete();
            }
            _dbSet.UpdateRange(entities);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task HardDeleteAsync(Guid id, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(id, includeDeleted: true, cancellationToken);
        if (entity != null)
        {
            await HardDeleteAsync(entity, saveChanges, cancellationToken);
        }
    }

    public async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await HardDeleteAsync(id, true, cancellationToken);
    }

    public async Task HardDeleteAsync(FileUploadEntity entity, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HardDeleteAsync(FileUploadEntity entity, CancellationToken cancellationToken = default)
    {
        await HardDeleteAsync(entity, true, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<FileUploadEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<IQueryable<FileUploadEntity>> GetOrderedQueryableAsync(Expression<Func<FileUploadEntity, object>> orderBy, SortDirection sortDirection = SortDirection.Ascending, bool includeDeleted = false)
    {
        var query = _dbSet.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = sortDirection == SortDirection.Ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        return await Task.FromResult(query);
    }

    public IQueryable<FileUploadEntity> ApplyOrderBy(IQueryable<FileUploadEntity> query, Expression<Func<FileUploadEntity, object>> orderBy, SortDirection sortDirection = SortDirection.Ascending)
    {
        return sortDirection == SortDirection.Ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);
    }

    public IQueryable<FileUploadEntity> ApplyPagination(IQueryable<FileUploadEntity> query, int pageNumber, int pageSize)
    {
        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    #endregion
}

