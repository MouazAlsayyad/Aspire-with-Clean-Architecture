using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Repositories;

/// <summary>
/// Cached implementation of IUserRepository.
/// Extends CachedRepository<User> and implements IUserRepository specific methods.
/// </summary>
public class CachedUserRepository : CachedRepository<User>, IUserRepository
{
    private readonly IUserRepository _userRepository;

    public CachedUserRepository(
        IUserRepository decorated,
        ICacheService cacheService,
        ILogger<CachedUserRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _userRepository = decorated;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"entity:User:email:{email.ToLowerInvariant()}";
        return _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _userRepository.GetByEmailAsync(email, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken
        );
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"entity:User:username:{userName.ToLowerInvariant()}";
        return _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _userRepository.GetByUserNameAsync(userName, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken
        );
    }

    public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        // We don't necessarily need to cache the boolean exists check, 
        // or we could cache it as well.
        return _userRepository.ExistsAsync(email, cancellationToken);
    }
}
