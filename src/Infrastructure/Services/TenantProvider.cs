using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services;

/// <summary>
/// Provides tenant context from HTTP headers or user claims.
/// </summary>
public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _currentTenantId;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentTenantId()
    {
        if (_currentTenantId.HasValue)
        {
            return _currentTenantId.Value;
        }

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            // This can happen during migrations, seeds, or background jobs
            // Throw an exception to indicate that HTTP context is not available
            // The global query filter in ApplicationDbContext checks IsHttpContextAvailable() to prevent this
            throw new InvalidOperationException("HTTP context is not available. This is expected during migrations, seeds, or background operations.");
        }

        // SECURITY: Get tenant ID ONLY from JWT claims (authenticated users)
        // Do NOT accept X-Tenant-Id header to prevent tenant spoofing attacks
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = httpContext.User.FindFirst("tenant_id");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var claimTenantId))
            {
                _currentTenantId = claimTenantId;
                return claimTenantId;
            }
            
            throw new InvalidOperationException("Authenticated user does not have a valid TenantId claim in the token.");
        }

        // For unauthenticated requests (e.g., login, public endpoints), allow X-Tenant-Id header
        // This is safe because these endpoints don't access tenant-specific data
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (int.TryParse(tenantIdHeader.FirstOrDefault(), out var tenantId))
            {
                _currentTenantId = tenantId;
                return tenantId;
            }
        }

        throw new InvalidOperationException("Tenant ID is not available. Please authenticate or provide X-Tenant-Id header for public endpoints.");
    }

    public void SetCurrentTenantId(int tenantId)
    {
        _currentTenantId = tenantId;
    }

    public bool IsHttpContextAvailable()
    {
        return _httpContextAccessor.HttpContext != null;
    }
}
