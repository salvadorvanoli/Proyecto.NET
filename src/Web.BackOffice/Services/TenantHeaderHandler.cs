using System.Security.Claims;

namespace Web.BackOffice.Services;

/// <summary>
/// DelegatingHandler that adds the TenantId header from the current user's claims to outgoing HTTP requests.
/// </summary>
public class TenantHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantHeaderHandler> _logger;

    public TenantHeaderHandler(IHttpContextAccessor httpContextAccessor, ILogger<TenantHeaderHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = httpContext.User.FindFirst("tenant_id");

            if (tenantIdClaim != null)
            {
                request.Headers.Add("X-Tenant-Id", tenantIdClaim.Value);
                _logger.LogDebug("Added TenantId header: {TenantId} to request {Uri}", tenantIdClaim.Value, request.RequestUri);
            }
            else
            {
                _logger.LogWarning("User is authenticated but TenantId claim is missing");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

