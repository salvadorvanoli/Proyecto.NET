using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Application.Common.Interfaces;

namespace Infrastructure.Services.Caching;

/// <summary>
/// Redis-based cache service implementation.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ICacheMetricsService _metricsService;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheOptions _options;

    public RedisCacheService(
        IDistributedCache cache,
        ICacheMetricsService metricsService,
        ILogger<RedisCacheService> logger,
        IOptions<CacheOptions> options)
    {
        _cache = cache;
        _metricsService = metricsService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Cache is disabled, returning null for key: {Key}", key);
            return null;
        }

        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);

            if (cachedValue == null)
            {
                _metricsService.RecordMiss(key);
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _metricsService.RecordHit(key);
            _logger.LogDebug("Cache hit for key: {Key}", key);

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Cache is disabled, skipping set for key: {Key}", key);
            return;
        }

        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromMinutes(_options.DefaultTtlMinutes)
            };

            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            _logger.LogDebug("Cached value for key: {Key} with TTL: {TTL}", key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Cache is disabled, skipping remove for key: {Key}", key);
            return;
        }

        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Cache is disabled, skipping pattern removal for pattern: {Pattern}", pattern);
            return Task.CompletedTask;
        }

        try
        {
            // Note: This is a simplified implementation
            // For production, you might want to use Redis SCAN command with pattern matching
            // This requires direct access to Redis, not IDistributedCache
            _logger.LogWarning("RemoveByPatternAsync is not fully implemented with IDistributedCache. Pattern: {Pattern}", pattern);
            
            // For now, we'll just log it
            // A full implementation would require StackExchange.Redis directly
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values by pattern from cache: {Pattern}", pattern);
        }
        
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return false;

        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            return value != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in cache for key: {Key}", key);
            return false;
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        
        if (cachedValue != null)
            return cachedValue;

        // If not in cache, execute factory
        var value = await factory();

        // Cache the result if not null
        if (value != null)
        {
            await SetAsync(key, value, ttl, cancellationToken);
        }

        return value;
    }
}

/// <summary>
/// Configuration options for caching.
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Indicates whether caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default TTL in minutes for cached items.
    /// </summary>
    public int DefaultTtlMinutes { get; set; } = 30;
}
