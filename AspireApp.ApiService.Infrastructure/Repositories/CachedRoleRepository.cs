using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Repositories;

/// <summary>
/// Cached implementation of IRoleRepository.
/// </summary>
public class CachedRoleRepository : CachedRepository<Role>, IRoleRepository
{
    private readonly IRoleRepository _roleRepository;

    public CachedRoleRepository(
        IRoleRepository decorated,
        ICacheService cacheService,
        ILogger<CachedRoleRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _roleRepository = decorated;
    }

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"entity:Role:name:{name.ToLowerInvariant()}";
        return _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _roleRepository.GetByNameAsync(name, ct),
            TimeSpanConstants.OneHour,
            cancellationToken
        );
    }
}
