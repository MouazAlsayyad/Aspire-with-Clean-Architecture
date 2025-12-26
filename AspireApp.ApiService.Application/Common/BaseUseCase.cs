using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AutoMapper;

namespace AspireApp.ApiService.Application.Common;

/// <summary>
/// Base class for use cases that require database persistence.
/// Automatically handles UnitOfWork SaveChangesAsync after successful execution.
/// </summary>
public abstract class BaseUseCase
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;

    protected BaseUseCase(IUnitOfWork unitOfWork, IMapper mapper)
    {
        UnitOfWork = unitOfWork;
        Mapper = mapper;
    }

    /// <summary>
    /// Executes the use case and automatically saves changes if the result is successful.
    /// </summary>
    protected async Task<Result<T>> ExecuteAndSaveAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        var result = await operation(cancellationToken);
        
        if (result.IsSuccess)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        
        return result;
    }

    /// <summary>
    /// Executes the use case and automatically saves changes if the result is successful.
    /// </summary>
    protected async Task<Result> ExecuteAndSaveAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken = default)
    {
        var result = await operation(cancellationToken);
        
        if (result.IsSuccess)
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        
        return result;
    }
}

