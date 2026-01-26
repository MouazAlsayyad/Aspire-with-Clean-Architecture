namespace AspireApp.Domain.Shared.Interfaces;

/// <summary>
/// Abstraction for distributed caching operations.
/// Provides a clean interface for caching that can be implemented by various cache providers.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached item by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached item</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The cached item or null if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a cached item with optional expiration.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <param name="ct">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <summary>
    /// Removes a cached item by key.
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Gets or creates a cached item using a factory function.
    /// If the item exists in cache, returns it. Otherwise, calls the factory to create it and caches the result.
    /// </summary>
    /// <typeparam name="T">The type of the cached item</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the item if not cached</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The cached or newly created item</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <summary>
    /// Removes all cached items matching a key pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match (e.g., "user:*")</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);
}
