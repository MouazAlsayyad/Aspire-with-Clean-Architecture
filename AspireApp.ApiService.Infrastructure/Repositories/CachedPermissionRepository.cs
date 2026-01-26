using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Repositories;

public class CachedPermissionRepository : CachedRepository<Permission>, IPermissionRepository
{
    private readonly IPermissionRepository _permissionRepository;

    public CachedPermissionRepository(
        IPermissionRepository decorated,
        ICacheService cacheService,
        ILogger<CachedPermissionRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _permissionRepository = decorated;
    }

    public Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"entity:Permission:name:{name.ToLowerInvariant()}";
        return _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _permissionRepository.GetByNameAsync(name, ct),
            TimeSpanConstants.OneHour,
            cancellationToken
        );
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"entity:Permission:resource:{resource.ToLowerInvariant()}";
        var result = await _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _permissionRepository.GetByResourceAsync(resource, ct),
            TimeSpanConstants.OneHour,
            cancellationToken
        );
        return result ?? Enumerable.Empty<Permission>();
    }

    public override async Task<Permission> UpdateAsync(Permission entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var updated = await base.UpdateAsync(entity, autoSave, cancellationToken);
        // Invalidate name/resource keys
        await _cacheService.RemoveAsync($"entity:Permission:name:{entity.Name.ToLowerInvariant()}", cancellationToken);
        await _cacheService.RemoveAsync($"entity:Permission:resource:{entity.Resource.ToLowerInvariant()}", cancellationToken);
        return updated;
    }

    public override async Task<Permission> UpdateAsync(Permission entity, CancellationToken cancellationToken)
    {
        var updated = await base.UpdateAsync(entity, cancellationToken);
        await _cacheService.RemoveAsync($"entity:Permission:name:{entity.Name.ToLowerInvariant()}", cancellationToken);
        await _cacheService.RemoveAsync($"entity:Permission:resource:{entity.Resource.ToLowerInvariant()}", cancellationToken);
        return updated;
    }
}
