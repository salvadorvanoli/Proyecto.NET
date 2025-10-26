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
            throw new InvalidOperationException("HTTP context is not available.");
        }

        // Try to get tenant ID from user claims (for authenticated users)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = httpContext.User.FindFirst("TenantId");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var claimTenantId))
            {
                _currentTenantId = claimTenantId;
                return claimTenantId;
            }
        }

        // Try to get tenant ID from header (for API calls)
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (int.TryParse(tenantIdHeader.FirstOrDefault(), out var tenantId))
            {
                _currentTenantId = tenantId;
                return tenantId;
            }
        }

        throw new InvalidOperationException("Tenant ID is not set. Please provide X-Tenant-Id header or authenticate with a tenant.");
    }

    public void SetCurrentTenantId(int tenantId)
    {
        _currentTenantId = tenantId;
    }
}
