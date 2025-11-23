using Shared.DTOs.Benefits;
using Application.Common.Interfaces;
using Application.Common.Caching;

namespace Application.Benefits;

/// <summary>
/// Decorator that adds caching capabilities to BenefitService.
/// Implements the Decorator pattern to cache frequently accessed benefit data.
/// </summary>
public class CachedBenefitService : IBenefitService
{
    private readonly IBenefitService _innerService;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public CachedBenefitService(
        IBenefitService innerService,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _innerService = innerService;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Gets a benefit by ID with caching.
    /// Cache key pattern: benefits:tenant:{tenantId}:id:{benefitId}
    /// TTL: 30 minutes
    /// </summary>
    public async Task<BenefitResponse?> GetBenefitByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.Benefits.ById(tenantId, id);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetBenefitByIdAsync(id, cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.ActiveBenefits),
            cancellationToken);
    }

    /// <summary>
    /// Gets user benefits without caching (user-specific data, less reusable).
    /// </summary>
    public Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId, CancellationToken cancellationToken = default)
    {
        // No caching for user-specific queries as cache hit rate would be low
        return _innerService.GetUserBenefitsAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Gets all benefits by tenant with caching.
    /// Cache key pattern: benefits:tenant:{tenantId}:all
    /// TTL: 30 minutes
    /// </summary>
    public async Task<IEnumerable<BenefitResponse>> GetBenefitsByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.Benefits.All(tenantId);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetBenefitsByTenantAsync(cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.ActiveBenefits),
            cancellationToken) ?? Enumerable.Empty<BenefitResponse>();
    }

    /// <summary>
    /// Gets benefits by type with caching.
    /// Cache key pattern: benefits:tenant:{tenantId}:type:{benefitTypeId}
    /// TTL: 30 minutes
    /// </summary>
    public async Task<IEnumerable<BenefitResponse>> GetBenefitsByTypeAsync(int benefitTypeId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.Benefits.ByType(tenantId, benefitTypeId);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetBenefitsByTypeAsync(benefitTypeId, cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.ActiveBenefits),
            cancellationToken) ?? Enumerable.Empty<BenefitResponse>();
    }

    /// <summary>
    /// Gets active benefits with caching - MOST FREQUENTLY ACCESSED IN FRONTOFFICE.
    /// Cache key pattern: benefits:tenant:{tenantId}:active
    /// TTL: 30 minutes
    /// </summary>
    public async Task<IEnumerable<BenefitResponse>> GetActiveBenefitsAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.Benefits.Active(tenantId);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetActiveBenefitsAsync(cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.ActiveBenefits),
            cancellationToken) ?? Enumerable.Empty<BenefitResponse>();
    }

    /// <summary>
    /// Gets available benefits for user - user-specific, no caching.
    /// This is personalized data with low cache hit rate.
    /// </summary>
    public Task<IEnumerable<AvailableBenefitResponse>> GetAvailableBenefitsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // No caching for user-specific queries
        return _innerService.GetAvailableBenefitsForUserAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Gets redeemable benefits for user - user-specific, no caching.
    /// This is personalized data with low cache hit rate.
    /// </summary>
    public Task<IEnumerable<RedeemableBenefitResponse>> GetRedeemableBenefitsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // No caching for user-specific queries
        return _innerService.GetRedeemableBenefitsForUserAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Creates a benefit and invalidates related cache entries.
    /// </summary>
    public async Task<BenefitResponse> CreateBenefitAsync(BenefitRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.CreateBenefitAsync(request, cancellationToken);

        // Invalidate cache for this tenant
        var tenantId = _tenantProvider.GetCurrentTenantId();
        await InvalidateBenefitCacheAsync(tenantId, cancellationToken);

        return result;
    }

    /// <summary>
    /// Updates a benefit and invalidates related cache entries.
    /// </summary>
    public async Task<BenefitResponse> UpdateBenefitAsync(int id, BenefitRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.UpdateBenefitAsync(id, request, cancellationToken);

        // Invalidate cache for this tenant and specific benefit
        var tenantId = _tenantProvider.GetCurrentTenantId();
        await InvalidateBenefitCacheAsync(tenantId, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.Benefits.ById(tenantId, id), cancellationToken);

        return result;
    }

    /// <summary>
    /// Deletes a benefit and invalidates related cache entries.
    /// </summary>
    public async Task<bool> DeleteBenefitAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.DeleteBenefitAsync(id, cancellationToken);

        if (result)
        {
            // Invalidate cache for this tenant and specific benefit
            var tenantId = _tenantProvider.GetCurrentTenantId();
            await InvalidateBenefitCacheAsync(tenantId, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.Benefits.ById(tenantId, id), cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Claims a benefit (user action) and invalidates cache.
    /// </summary>
    public async Task<ClaimBenefitResponse> ClaimBenefitAsync(ClaimBenefitRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.ClaimBenefitAsync(request, cancellationToken);

        // Invalidate cache as quotas have changed
        var tenantId = _tenantProvider.GetCurrentTenantId();
        await InvalidateBenefitCacheAsync(tenantId, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.Benefits.ById(tenantId, request.BenefitId), cancellationToken);

        return result;
    }

    /// <summary>
    /// Redeems a benefit (user action) and invalidates cache.
    /// </summary>
    public async Task<RedeemBenefitResponse> RedeemBenefitAsync(RedeemBenefitRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.RedeemBenefitAsync(request, cancellationToken);

        // Invalidate cache as quantities have changed
        var tenantId = _tenantProvider.GetCurrentTenantId();
        await InvalidateBenefitCacheAsync(tenantId, cancellationToken);

        return result;
    }

    /// <summary>
    /// Invalidates all benefit cache entries for a specific tenant.
    /// Uses pattern matching to remove all benefit-related keys.
    /// </summary>
    private async Task InvalidateBenefitCacheAsync(int tenantId, CancellationToken cancellationToken)
    {
        var pattern = CacheKeys.Benefits.Pattern(tenantId);
        await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
    }
}
