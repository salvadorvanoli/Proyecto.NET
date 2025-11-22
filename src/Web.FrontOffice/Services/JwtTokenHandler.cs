using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Web.FrontOffice.Services;

/// <summary>
/// DelegatingHandler que agrega el JWT token desde cookies HTTP (AuthenticationProperties)
/// </summary>
public class JwtTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<JwtTokenHandler> _logger;

    public JwtTokenHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<JwtTokenHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Obtener el token JWT almacenado en las propiedades de autenticación
                var token = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug("Added JWT token to request {Uri}", request.RequestUri);
                }

                // Agregar TenantId header
                var tenantIdClaim = httpContext.User.FindFirst("TenantId");
                if (tenantIdClaim != null)
                {
                    request.Headers.Add("X-Tenant-Id", tenantIdClaim.Value);
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
