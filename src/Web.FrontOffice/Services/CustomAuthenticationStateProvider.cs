using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Web.FrontOffice.Services;

/// <summary>
/// AuthenticationStateProvider simple que mantiene el estado en memoria para el circuito de Blazor Server.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private AuthenticationState _authenticationState = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private string? _jwtToken;

    public CustomAuthenticationStateProvider(ILogger<CustomAuthenticationStateProvider> logger)
    {
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_authenticationState);
    }
    
    public void MarkUserAsAuthenticated(string jwtToken)
    {
        try
        {
            _jwtToken = jwtToken;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            
            var identity = new ClaimsIdentity(token.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            _authenticationState = new AuthenticationState(user);
            
            _logger.LogInformation("User authenticated: {Name}", user.Identity?.Name);
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as authenticated");
        }
    }
    
    public void MarkUserAsLoggedOut()
    {
        _jwtToken = null;
        _authenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        
        _logger.LogInformation("User logged out");
        NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
    }
    
    public string? GetJwtToken()
    {
        return _jwtToken;
    }

    public async Task<int?> GetTenantIdAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        var tenantIdClaim = authState.User.FindFirst("TenantId");

        if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return tenantId;
        }

        return null;
    }

    public async Task<int?> GetUserIdAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}