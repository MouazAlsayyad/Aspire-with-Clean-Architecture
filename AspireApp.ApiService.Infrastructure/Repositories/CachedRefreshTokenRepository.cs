using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Authentication.Entities;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Repositories;

public class CachedRefreshTokenRepository : CachedRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public CachedRefreshTokenRepository(
        IRefreshTokenRepository decorated,
        ICacheService cacheService,
        ILogger<CachedRefreshTokenRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _refreshTokenRepository = decorated;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Refresh tokens are sensitive and one-time use. Caching them might be risky if not invalidated properly.
        // However, we can cache them for short duration or until revocation.
        var cacheKey = $"entity:RefreshToken:token:{token}";
        return _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _refreshTokenRepository.GetByTokenAsync(token, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken
        );
    }

    public Task<RefreshToken?> GetByTokenIncludeRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        // Similar to GetByTokenAsync but includes revoked ones.
        // We can cache this too, but key should be different.
        var cacheKey = $"entity:RefreshToken:token_revoked:{token}";
        return _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _refreshTokenRepository.GetByTokenIncludeRevokedAsync(token, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken
        );
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"entity:RefreshToken:user:{userId}";
        var result = await _cacheService.GetOrSetAsync(
            cacheKey,
            async ct => await _refreshTokenRepository.GetByUserIdAsync(userId, ct),
            TimeSpanConstants.FiveMinutes,
            cancellationToken
        );
        return result ?? new List<RefreshToken>();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, cancellationToken);
        // Invalidate user's token list
        await _cacheService.RemoveAsync($"entity:RefreshToken:user:{userId}", cancellationToken);
    }

    public Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        return _refreshTokenRepository.CleanupExpiredTokensAsync(cancellationToken);
    }

    public override async Task<RefreshToken> UpdateAsync(RefreshToken entity, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var updated = await base.UpdateAsync(entity, autoSave, cancellationToken);
        await _cacheService.RemoveAsync($"entity:RefreshToken:token:{entity.Token}", cancellationToken);
        await _cacheService.RemoveAsync($"entity:RefreshToken:token_revoked:{entity.Token}", cancellationToken);
        await _cacheService.RemoveAsync($"entity:RefreshToken:user:{entity.UserId}", cancellationToken);
        return updated;
    }

    public override async Task<RefreshToken> UpdateAsync(RefreshToken entity, CancellationToken cancellationToken)
    {
        var updated = await base.UpdateAsync(entity, cancellationToken);
        await _cacheService.RemoveAsync($"entity:RefreshToken:token:{entity.Token}", cancellationToken);
        await _cacheService.RemoveAsync($"entity:RefreshToken:token_revoked:{entity.Token}", cancellationToken);
        await _cacheService.RemoveAsync($"entity:RefreshToken:user:{entity.UserId}", cancellationToken);
        return updated;
    }
}
