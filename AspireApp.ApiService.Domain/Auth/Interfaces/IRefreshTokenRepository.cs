using AspireApp.ApiService.Domain.Auth.Entities;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Domain.Auth.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenIncludeRevokedAsync(string token, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}

