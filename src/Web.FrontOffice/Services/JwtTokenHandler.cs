using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Web.FrontOffice.Services;

/// <summary>
/// DelegatingHandler that adds the JWT Bearer token and TenantId header to outgoing HTTP requests.
/// This handler is used to automatically include authentication information in API calls.
/// </summary>
public class JwtTokenHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<JwtTokenHandler> _logger;

    public JwtTokenHandler(
        AuthenticationStateProvider authStateProvider,
        ILogger<JwtTokenHandler> logger)
    {
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                // Add JWT Authorization token
                var token = user.FindFirst("access_token")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug("Added Authorization Bearer token to request {Uri}", request.RequestUri);
                }
                else
                {
                    _logger.LogWarning("User is authenticated but access_token claim is missing for request {Uri}", request.RequestUri);
                }

                // Add TenantId header
                var tenantIdClaim = user.FindFirst("TenantId");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding authentication headers to request {Uri}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
