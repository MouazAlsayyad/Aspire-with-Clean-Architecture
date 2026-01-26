using AspireApp.Domain.Shared.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AspireApp.ApiService.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(key, ct);
        return cached is null ? default : JsonSerializer.Deserialize<T>(cached, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(key, json, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null)
        {
            return cached;
        }
        var value = await factory(ct);
        if (value is not null)
        {
            await SetAsync(key, value, expiration, ct);
        }
        
        return value;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        // IDistributedCache does not support pattern removal directly.
        // This would require IConnectionMultiplexer which is not injected here.
        // For now we will just throw as this specific implementation focuses on IDistributedCache.
        throw new NotSupportedException("Pattern removal is not supported by the default RedisCacheService using IDistributedCache.");
    }
}
