using Microsoft.AspNetCore.Authentication;

namespace Web.BackOffice.Services;

/// <summary>
/// DelegatingHandler that adds the JWT Bearer token and TenantId header from the current user to outgoing HTTP requests.
/// </summary>
public class JwtTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<JwtTokenHandler> _logger;

    public JwtTokenHandler(IHttpContextAccessor httpContextAccessor, ILogger<JwtTokenHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Agregar token JWT de Authorization
            var accessToken = await httpContext.GetTokenAsync("access_token");
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                _logger.LogDebug("Added Authorization Bearer token to request {Uri}", request.RequestUri);
            }
            else
            {
                _logger.LogWarning("User is authenticated but access_token is missing for request {Uri}", request.RequestUri);
            }

            // Agregar header de TenantId (buscar en las cookies del BackOffice)
            var tenantIdClaim = httpContext.User.FindFirst("TenantId");
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
        else
        {
            _logger.LogDebug("User is not authenticated for request {Uri}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
