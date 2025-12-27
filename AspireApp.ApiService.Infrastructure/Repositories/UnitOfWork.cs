using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AspireApp.ApiService.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle specific concurrency conflicts based on entity state
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is BaseEntity)
                {
                    // Skip Added entities - they don't have original values in the database
                    if (entry.State == EntityState.Added)
                    {
                        continue;
                    }

                    var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                    if (databaseValues == null)
                    {
                        // Entity no longer exists in database - detach it
                        entry.State = EntityState.Detached;
                        continue;
                    }

                    // Entity exists in database - refresh original values to resolve conflict (Client Wins strategy)
                    entry.OriginalValues.SetValues(databaseValues);
                }
            }

            // Retry SaveChanges after handling the conflicts
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public IRepository<T> GetRepository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (_repositories.TryGetValue(type, out var repository))
        {
            return (IRepository<T>)repository;
        }

        var newRepository = new Repository<T>(_context);
        _repositories[type] = newRepository;
        return newRepository;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
    }

}

