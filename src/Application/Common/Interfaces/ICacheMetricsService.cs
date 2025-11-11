namespace Application.Common.Interfaces;

/// <summary>
/// Interface for tracking cache metrics (hit/miss rates).
/// </summary>
public interface ICacheMetricsService
{
    /// <summary>
    /// Records a cache hit.
    /// </summary>
    void RecordHit(string key);

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    void RecordMiss(string key);

    /// <summary>
    /// Gets the current cache statistics.
    /// </summary>
    CacheMetrics GetMetrics();

    /// <summary>
    /// Gets cache statistics for a specific key pattern.
    /// </summary>
    CacheMetrics GetMetrics(string keyPattern);

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    void Reset();
}

/// <summary>
/// Cache metrics data.
/// </summary>
public class CacheMetrics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public long TotalRequests => TotalHits + TotalMisses;
    public double HitRate => TotalRequests > 0 ? (double)TotalHits / TotalRequests : 0;
    public double MissRate => TotalRequests > 0 ? (double)TotalMisses / TotalRequests : 0;
    public Dictionary<string, long> HitsByPattern { get; set; } = new();
    public Dictionary<string, long> MissesByPattern { get; set; } = new();
}
