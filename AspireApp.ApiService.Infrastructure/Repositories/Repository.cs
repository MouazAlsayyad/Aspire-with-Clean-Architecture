using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace AspireApp.ApiService.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation.
/// Does not automatically save changes - UnitOfWork handles transaction management.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Methods

    public virtual async Task<T?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // If including deleted, ignore the global query filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<T?> FindAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return await GetAsync(id, includeDeleted, cancellationToken);
    }

    public virtual async Task<List<T>> GetListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // If including deleted, ignore the global query filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // If including deleted, ignore the global query filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query.Where(predicate);

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IQueryable<T>> GetQueryableAsync(bool ignoreFilters = false)
    {
        var query = _dbSet.AsQueryable();

        // If ignoreFilters is true, ignore the global query filter
        if (ignoreFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return await Task.FromResult(query);
    }

    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _dbSet.LongCountAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _dbSet
            .Where(predicate)
            .LongCountAsync(cancellationToken);
    }

    public virtual async Task<T> InsertAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        entity.SetCreationTime();
        await _dbSet.AddAsync(entity, cancellationToken);

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    public virtual async Task<T> InsertAsync(T entity, CancellationToken cancellationToken)
    {
        return await InsertAsync(entity, autoSave: false, cancellationToken);
    }

    public virtual async Task InsertManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.SetCreationTime();
        }

        await _dbSet.AddRangeAsync(entityList, cancellationToken);

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<T> UpdateAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            entity.SetLastModificationTime();
            _dbSet.Update(entity); // Attaches and marks as Modified
        }
        else
        {
            // Entity is already tracked. 
            // Ensure modification metadata is updated.
            entity.SetLastModificationTime();

            // Ensure new entities added to navigation property collections are tracked by EF Core
            // This is a general solution that works for all entities with collection navigation properties
            EnsureNavigationPropertiesAreTracked(entry);
        }

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    /// <summary>
    /// Ensures that new entities added to navigation property collections are tracked by EF Core.
    /// This is a general solution that works for all entities following ABP Framework patterns.
    /// </summary>
    private void EnsureNavigationPropertiesAreTracked(EntityEntry entry)
    {
        // Get all collection navigation properties
        var collectionNavigations = entry.Navigations
            .Where(n => n.Metadata.IsCollection)
            .ToList();

        foreach (var navigation in collectionNavigations)
        {
            // Check if the collection is loaded
            if (!navigation.IsLoaded)
            {
                continue; // Skip unloaded collections
            }

            // Get the collection value
            var collectionValue = navigation.CurrentValue;
            if (collectionValue == null)
            {
                continue;
            }

            // Use reflection to get the actual collection items
            // Collections can be IEnumerable<T> where T is BaseEntity
            var itemType = navigation.Metadata.TargetEntityType.ClrType;
            if (!typeof(BaseEntity).IsAssignableFrom(itemType))
            {
                continue; // Skip if items are not BaseEntity
            }

            // Convert to enumerable and check each item
            var items = ((System.Collections.IEnumerable)collectionValue).Cast<BaseEntity>().ToList();

            foreach (var item in items)
            {
                var itemEntry = _context.Entry(item);
                if (itemEntry.State == EntityState.Detached)
                {
                    // New entity in collection - add it to the context so EF Core tracks it
                    // Use reflection to get the appropriate DbSet and add the entity
                    var dbSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes);
                    var genericDbSetMethod = dbSetMethod?.MakeGenericMethod(itemType);
                    var dbSet = genericDbSetMethod?.Invoke(_context, null);

                    if (dbSet != null)
                    {
                        var addMethod = dbSet.GetType().GetMethod(nameof(DbSet<BaseEntity>.Add));
                        if (addMethod != null)
                        {
                            addMethod.Invoke(dbSet, new object[] { item });
                        }
                    }
                }
            }
        }
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        return await UpdateAsync(entity, autoSave: false, cancellationToken);
    }

    public virtual async Task UpdateManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.SetLastModificationTime();
        }

        _dbSet.UpdateRange(entityList);

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(id, includeDeleted: false, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, autoSave, cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await DeleteAsync(id, autoSave: false, cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        entity.Delete();
        _dbSet.Update(entity);

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        await DeleteAsync(entity, autoSave: false, cancellationToken);
    }

    public virtual async Task DeleteManyAsync(IEnumerable<T> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.Delete();
        }

        _dbSet.UpdateRange(entityList);

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task HardDeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            await HardDeleteAsync(entity, autoSave, cancellationToken);
        }
    }

    public virtual async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await HardDeleteAsync(id, autoSave: false, cancellationToken);
    }

    public virtual async Task HardDeleteAsync(T entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);

        if (autoSave)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task HardDeleteAsync(T entity, CancellationToken cancellationToken)
    {
        await HardDeleteAsync(entity, autoSave: false, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _dbSet
            .AnyAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _dbSet
            .AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<IQueryable<T>> GetOrderedQueryableAsync(
        Expression<Func<T, object>> orderBy,
        SortDirection sortDirection = SortDirection.Ascending,
        bool includeDeleted = false)
    {
        var query = _dbSet.AsQueryable();

        // If including deleted, ignore the global query filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        query = ApplyOrderBy(query, orderBy, sortDirection);

        return await Task.FromResult(query);
    }

    public virtual IQueryable<T> ApplyOrderBy(
        IQueryable<T> query,
        Expression<Func<T, object>> orderBy,
        SortDirection sortDirection = SortDirection.Ascending)
    {
        if (sortDirection == SortDirection.Descending)
        {
            return query.OrderByDescending(orderBy);
        }

        return query.OrderBy(orderBy);
    }

    public virtual IQueryable<T> ApplyPagination(
        IQueryable<T> query,
        int pageNumber, int pageSize)
    {
        PaginationHelper.Normalize(ref pageNumber, ref pageSize);
        return query.Skip(PaginationHelper.Skip(pageNumber, pageSize)).Take(PaginationHelper.Take(pageSize));
    }

    /// <summary>
    /// Applies ordering by property name using reflection.
    /// This is a fallback when orderBy expression is not provided but SortBy property name is.
    /// </summary>
    private IQueryable<T> ApplyOrderByPropertyName(
        IQueryable<T> query,
        string propertyName,
        SortDirection sortDirection)
    {
        var propertyInfo = typeof(T).GetProperty(
            propertyName,
            System.Reflection.BindingFlags.IgnoreCase |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        if (propertyInfo == null)
        {
            // If property not found, return query as-is (or throw exception)
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyInfo);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = sortDirection == SortDirection.Descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), propertyInfo.PropertyType },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    #endregion

}

