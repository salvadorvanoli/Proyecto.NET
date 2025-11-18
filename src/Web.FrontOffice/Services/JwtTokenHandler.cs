using System.Security.Claims;

namespace Web.FrontOffice.Services;

/// <summary>
/// DelegatingHandler que agrega el JWT token desde CustomAuthenticationStateProvider
/// </summary>
public class JwtTokenHandler : DelegatingHandler
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly ILogger<JwtTokenHandler> _logger;

    public JwtTokenHandler(
        CustomAuthenticationStateProvider authStateProvider,
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
            var token = _authStateProvider.GetJwtToken();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Added JWT token to request {Uri}", request.RequestUri);
                
                // Agregar TenantId header
                var tenantId = await _authStateProvider.GetTenantIdAsync();
                if (tenantId.HasValue)
                {
                    request.Headers.Add("X-Tenant-Id", tenantId.Value.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding JWT token to request");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
