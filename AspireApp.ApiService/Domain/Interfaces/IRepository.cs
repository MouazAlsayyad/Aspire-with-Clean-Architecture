using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using System.Linq.Expressions;

namespace AspireApp.ApiService.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its ID. Returns null if not found or deleted (unless includeDeleted is true).
    /// </summary>
    Task<T?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an entity by its ID. Returns null if not found (doesn't throw exception).
    /// </summary>
    Task<T?> FindAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of entities.
    /// </summary>
    Task<List<T>> GetListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of entities matching the predicate.
    /// </summary>
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a queryable for building custom queries.
    /// </summary>
    /// <param name="ignoreFilters">If true, ignores global query filters (e.g., soft delete filter)</param>
    Task<IQueryable<T>> GetQueryableAsync(bool ignoreFilters = false);

    /// <summary>
    /// Gets count of entities.
    /// </summary>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of entities matching the predicate.
    /// </summary>
    Task<long> GetCountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity. 
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task<T> InsertAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity without auto-save.
    /// </summary>
    Task<T> InsertAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts multiple entities. 
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task InsertManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity. 
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task<T> UpdateAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity without auto-save. 
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Updates multiple entities.
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task UpdateManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by ID (soft delete). 
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task DeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by ID without auto-save. 
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an entity (soft delete). 
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task DeleteAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity without auto-save.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes multiple entities (soft delete).
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task DeleteManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes an entity (permanently removes from database).
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task HardDeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes an entity by ID without auto-save.
    /// </summary>
    Task HardDeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Hard deletes an entity (permanently removes from database).
    /// Note: Does not save changes automatically (UnitOfWork handles it)
    /// </summary>
    Task HardDeleteAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes an entity without auto-save.
    /// </summary>
    Task HardDeleteAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an entity exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a queryable with ordering applied.
    /// </summary>
    /// <param name="orderBy">Expression to order by</param>
    /// <param name="sortDirection">Sort direction (ascending or descending)</param>
    /// <param name="includeDeleted">Whether to include deleted entities</param>
    Task<IQueryable<T>> GetOrderedQueryableAsync(
        Expression<Func<T, object>> orderBy,
        SortDirection sortDirection = SortDirection.Ascending,
        bool includeDeleted = false);

    /// <summary>
    /// Applies ordering to an existing queryable.
    /// </summary>
    /// <param name="query">The queryable source</param>
    /// <param name="orderBy">Expression to order by</param>
    /// <param name="sortDirection">Sort direction (ascending or descending)</param>
    IQueryable<T> ApplyOrderBy(
        IQueryable<T> query,
        Expression<Func<T, object>> orderBy,
        SortDirection sortDirection = SortDirection.Ascending);

    /// <summary>
    /// Applies pagination (Skip/Take) to an existing queryable.
    /// </summary>
    /// <param name="query">The queryable source</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Queryable with Skip and Take applied</returns>
    IQueryable<T> ApplyPagination(
        IQueryable<T> query,
        int pageNumber,
        int pageSize);
}

