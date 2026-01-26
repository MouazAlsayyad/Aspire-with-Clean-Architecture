using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AspireApp.ApiService.Infrastructure.Repositories;

public class CachedRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IRepository<T> _repository;
    protected readonly ICacheService _cacheService;
    protected readonly ILogger _logger;

    public CachedRepository(IRepository<T> repository, ICacheService cacheService, ILogger logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public virtual async Task<T?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (includeDeleted)
        {
            return await _repository.GetAsync(id, includeDeleted, cancellationToken);
        }

        string key = GetCacheKey(id);
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _repository.GetAsync(id, includeDeleted, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken);
    }

    public virtual async Task<T?> FindAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (includeDeleted)
        {
            return await _repository.FindAsync(id, includeDeleted, cancellationToken);
        }

        string key = GetCacheKey(id);

        return await _cacheService.GetOrSetAsync(
            key,
            ct => _repository.FindAsync(id, includeDeleted, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken);
    }

    public virtual Task<List<T>> GetListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return _repository.GetListAsync(includeDeleted, cancellationToken);
    }

    public virtual Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return _repository.GetListAsync(predicate, includeDeleted, cancellationToken);
    }

    public virtual Task<IQueryable<T>> GetQueryableAsync(bool ignoreFilters = false)
    {
        return _repository.GetQueryableAsync(ignoreFilters);
    }

    public virtual Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetCountAsync(cancellationToken);
    }

    public virtual Task<long> GetCountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _repository.GetCountAsync(predicate, cancellationToken);
    }

    public virtual Task<T> InsertAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        return _repository.InsertAsync(entity, autoSave, cancellationToken);
    }

    public virtual Task<T> InsertAsync(T entity, CancellationToken cancellationToken)
    {
        return _repository.InsertAsync(entity, cancellationToken);
    }

    public virtual Task InsertManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        return _repository.InsertManyAsync(entities, autoSave, cancellationToken);
    }

    public virtual async Task<T> UpdateAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var updatedEntity = await _repository.UpdateAsync(entity, autoSave, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
        return updatedEntity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
        return updatedEntity;
    }

    public virtual async Task UpdateManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateManyAsync(entities, autoSave, cancellationToken);
        foreach (var entity in entities)
        {
            await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, autoSave, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(id), cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(id), cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(entity, autoSave, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(entity, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
    }

    public virtual async Task DeleteManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteManyAsync(entities, autoSave, cancellationToken);
        foreach (var entity in entities)
        {
            await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
        }
    }

    public virtual async Task HardDeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        await _repository.HardDeleteAsync(id, autoSave, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(id), cancellationToken);
    }

    public virtual async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _repository.HardDeleteAsync(id, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(id), cancellationToken);
    }

    public virtual async Task HardDeleteAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        await _repository.HardDeleteAsync(entity, autoSave, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
    }

    public virtual async Task HardDeleteAsync(T entity, CancellationToken cancellationToken)
    {
        await _repository.HardDeleteAsync(entity, cancellationToken);
        await _cacheService.RemoveAsync(GetCacheKey(entity.Id), cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(predicate, cancellationToken);
    }

    public virtual Task<IQueryable<T>> GetOrderedQueryableAsync(Expression<Func<T, object>> orderBy, SortDirection sortDirection = SortDirection.Ascending, bool includeDeleted = false)
    {
        return _repository.GetOrderedQueryableAsync(orderBy, sortDirection, includeDeleted);
    }

    public virtual IQueryable<T> ApplyOrderBy(IQueryable<T> query, Expression<Func<T, object>> orderBy, SortDirection sortDirection = SortDirection.Ascending)
    {
        return _repository.ApplyOrderBy(query, orderBy, sortDirection);
    }

    public virtual IQueryable<T> ApplyPagination(IQueryable<T> query, int pageNumber, int pageSize)
    {
        return _repository.ApplyPagination(query, pageNumber, pageSize);
    }

    protected static string GetCacheKey(Guid id)
    {
        return $"{typeof(T).Name}-{id}";
    }
}
