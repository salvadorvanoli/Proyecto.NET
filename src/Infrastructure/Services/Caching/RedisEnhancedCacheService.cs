using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using Application.Common.Interfaces;

namespace Infrastructure.Services.Caching;

/// <summary>
/// Enhanced Redis cache service with pattern matching support using StackExchange.Redis.
/// </summary>
public class RedisEnhancedCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ICacheMetricsService _metricsService;
    private readonly ILogger<RedisEnhancedCacheService> _logger;
    private readonly CacheOptions _options;
    private readonly IDatabase _database;

    public RedisEnhancedCacheService(
        IConnectionMultiplexer redis,
        ICacheMetricsService metricsService,
        ILogger<RedisEnhancedCacheService> logger,
        IOptions<CacheOptions> options)
    {
        _redis = redis;
        _metricsService = metricsService;
        _logger = logger;
        _options = options.Value;
        _database = _redis.GetDatabase();
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
            var cachedValue = await _database.StringGetAsync(key);

            if (cachedValue.IsNullOrEmpty)
            {
                _metricsService.RecordMiss(key);
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _metricsService.RecordHit(key);
            _logger.LogDebug("Cache hit for key: {Key}", key);

            return JsonSerializer.Deserialize<T>(cachedValue!);
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
            var expiry = ttl ?? TimeSpan.FromMinutes(_options.DefaultTtlMinutes);

            await _database.StringSetAsync(key, serializedValue, expiry);
            _logger.LogDebug("Cached value for key: {Key} with TTL: {TTL}", key, expiry);
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
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Cache is disabled, skipping pattern removal for pattern: {Pattern}", pattern);
            return;
        }

        try
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            var keys = server.Keys(pattern: pattern).ToArray();
            
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogInformation("Removed {Count} cached values matching pattern: {Pattern}", keys.Length, pattern);
            }
            else
            {
                _logger.LogDebug("No cached values found matching pattern: {Pattern}", pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values by pattern from cache: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return false;

        try
        {
            return await _database.KeyExistsAsync(key);
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
