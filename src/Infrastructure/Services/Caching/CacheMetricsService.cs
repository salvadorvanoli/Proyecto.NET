using System.Collections.Concurrent;
using Application.Common.Interfaces;

namespace Infrastructure.Services.Caching;

/// <summary>
/// Service for tracking cache hit/miss metrics.
/// </summary>
public class CacheMetricsService : ICacheMetricsService
{
    private long _totalHits;
    private long _totalMisses;
    private readonly ConcurrentDictionary<string, long> _hitsByPattern = new();
    private readonly ConcurrentDictionary<string, long> _missesByPattern = new();

    public void RecordHit(string key)
    {
        Interlocked.Increment(ref _totalHits);
        
        var pattern = ExtractPattern(key);
        _hitsByPattern.AddOrUpdate(pattern, 1, (_, count) => count + 1);
    }

    public void RecordMiss(string key)
    {
        Interlocked.Increment(ref _totalMisses);
        
        var pattern = ExtractPattern(key);
        _missesByPattern.AddOrUpdate(pattern, 1, (_, count) => count + 1);
    }

    public CacheMetrics GetMetrics()
    {
        return new CacheMetrics
        {
            TotalHits = Interlocked.Read(ref _totalHits),
            TotalMisses = Interlocked.Read(ref _totalMisses),
            HitsByPattern = new Dictionary<string, long>(_hitsByPattern),
            MissesByPattern = new Dictionary<string, long>(_missesByPattern)
        };
    }

    public CacheMetrics GetMetrics(string keyPattern)
    {
        var pattern = NormalizePattern(keyPattern);
        
        return new CacheMetrics
        {
            TotalHits = _hitsByPattern.TryGetValue(pattern, out var hits) ? hits : 0,
            TotalMisses = _missesByPattern.TryGetValue(pattern, out var misses) ? misses : 0,
            HitsByPattern = _hitsByPattern.Where(x => x.Key == pattern).ToDictionary(x => x.Key, x => x.Value),
            MissesByPattern = _missesByPattern.Where(x => x.Key == pattern).ToDictionary(x => x.Key, x => x.Value)
        };
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _totalHits, 0);
        Interlocked.Exchange(ref _totalMisses, 0);
        _hitsByPattern.Clear();
        _missesByPattern.Clear();
    }

    /// <summary>
    /// Extracts the pattern from a cache key (e.g., "benefits:tenant:1:active" -> "benefits").
    /// </summary>
    private static string ExtractPattern(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return "unknown";

        var firstColon = key.IndexOf(':');
        return firstColon > 0 ? key[..firstColon] : key;
    }

    /// <summary>
    /// Normalizes a pattern for lookup (removes wildcards).
    /// </summary>
    private static string NormalizePattern(string pattern)
    {
        return pattern.Replace("*", "").TrimEnd(':');
    }
}
