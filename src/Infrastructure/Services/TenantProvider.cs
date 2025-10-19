using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

/// <summary>
/// Provides tenant context from HTTP headers.
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

        // Try to get tenant ID from header
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) == true)
        {
            if (int.TryParse(tenantIdHeader.FirstOrDefault(), out var tenantId))
            {
                _currentTenantId = tenantId;
                return tenantId;
            }
        }

        throw new InvalidOperationException("Tenant ID is not set. Please provide X-Tenant-Id header.");
    }

    public void SetCurrentTenantId(int tenantId)
    {
        _currentTenantId = tenantId;
    }
}
