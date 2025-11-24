using Shared.DTOs.AccessRules;
using Shared.DTOs;
using Application.Common.Interfaces;
using Application.Common.Caching;

namespace Application.AccessRules;

/// <summary>
/// Decorator that adds caching capabilities to AccessRuleService.
/// Implements the Decorator pattern to cache frequently accessed access control data.
/// </summary>
public class CachedAccessRuleService : IAccessRuleService
{
    private readonly IAccessRuleService _innerService;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public CachedAccessRuleService(
        IAccessRuleService innerService,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _innerService = innerService;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Gets an access rule by ID with caching.
    /// Cache key pattern: accessrules:tenant:{tenantId}:id:{ruleId}
    /// TTL: 60 minutes (access rules change less frequently)
    /// </summary>
    public async Task<AccessRuleResponse?> GetAccessRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.AccessRules.ById(tenantId, id);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetAccessRuleByIdAsync(id, cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.AccessRules),
            cancellationToken);
    }

    /// <summary>
    /// Gets all access rules by tenant with caching.
    /// Cache key pattern: accessrules:tenant:{tenantId}:all
    /// TTL: 60 minutes
    /// </summary>
    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.AccessRules.All(tenantId);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetAccessRulesByTenantAsync(cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.AccessRules),
            cancellationToken) ?? Enumerable.Empty<AccessRuleResponse>();
    }

    /// <summary>
    /// Gets access rules by control point with caching.
    /// Cache key pattern: accessrules:tenant:{tenantId}:controlpoint:{controlPointId}
    /// TTL: 60 minutes
    /// </summary>
    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByControlPointAsync(int controlPointId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.AccessRules.ByControlPoint(tenantId, controlPointId);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetAccessRulesByControlPointAsync(controlPointId, cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.AccessRules),
            cancellationToken) ?? Enumerable.Empty<AccessRuleResponse>();
    }

    /// <summary>
    /// Gets access rules by role without caching.
    /// Role-based queries are less frequent and more dynamic.
    /// </summary>
    public Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        // No caching for role-based queries as they're less common
        return _innerService.GetAccessRulesByRoleAsync(roleId, cancellationToken);
    }

    /// <summary>
    /// Creates an access rule and invalidates related cache entries.
    /// </summary>
    public async Task<AccessRuleResponse> CreateAccessRuleAsync(AccessRuleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.CreateAccessRuleAsync(request, cancellationToken);

        // Invalidate cache for this tenant
        var tenantId = _tenantProvider.GetCurrentTenantId();
        await InvalidateAccessRuleCacheAsync(tenantId, cancellationToken);

        return result;
    }

    /// <summary>
    /// Updates an access rule and invalidates related cache entries.
    /// </summary>
    public async Task<AccessRuleResponse> UpdateAccessRuleAsync(int id, AccessRuleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.UpdateAccessRuleAsync(id, request, cancellationToken);

        // Invalidate cache for this tenant and specific rule
        var tenantId = _tenantProvider.GetCurrentTenantId();
        await InvalidateAccessRuleCacheAsync(tenantId, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.AccessRules.ById(tenantId, id), cancellationToken);

        return result;
    }

    /// <summary>
    /// Deletes an access rule and invalidates related cache entries.
    /// </summary>
    public async Task<bool> DeleteAccessRuleAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.DeleteAccessRuleAsync(id, cancellationToken);

        if (result)
        {
            // Invalidate cache for this tenant and specific rule
            var tenantId = _tenantProvider.GetCurrentTenantId();
            await InvalidateAccessRuleCacheAsync(tenantId, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.AccessRules.ById(tenantId, id), cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Gets all active access rules with caching - CRITICAL FOR MOBILE OFFLINE SYNC.
    /// Cache key pattern: accessrules:tenant:{tenantId}:active
    /// TTL: 60 minutes
    /// </summary>
    public async Task<List<AccessRuleDto>> GetAllActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        var cacheKey = CacheKeys.AccessRules.Active(tenantId);

        var result = await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _innerService.GetAllActiveRulesAsync(cancellationToken),
            TimeSpan.FromMinutes(CacheKeys.Ttl.AccessRules),
            cancellationToken);

        return result ?? new List<AccessRuleDto>();
    }

    /// <summary>
    /// Invalidates all access rule cache entries for a specific tenant.
    /// Uses pattern matching to remove all access rule-related keys.
    /// </summary>
    private async Task InvalidateAccessRuleCacheAsync(int tenantId, CancellationToken cancellationToken)
    {
        var pattern = CacheKeys.AccessRules.Pattern(tenantId);
        await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
    }
}
