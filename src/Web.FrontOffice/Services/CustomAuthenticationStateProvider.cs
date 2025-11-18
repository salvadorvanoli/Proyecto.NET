using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;
using Shared.DTOs.Auth;

namespace Web.FrontOffice.Services;

/// <summary>
/// Custom authentication state provider that manages user authentication state
/// using ProtectedBrowserStorage (localStorage) for Blazor Server.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _protectedLocalStore;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private const string USER_SESSION_KEY = "userSession";

    public CustomAuthenticationStateProvider(
        ProtectedLocalStorage protectedLocalStore,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _protectedLocalStore = protectedLocalStore;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current authentication state.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var result = await _protectedLocalStore.GetAsync<string>(USER_SESSION_KEY);

            if (!result.Success || string.IsNullOrEmpty(result.Value))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result.Value);

            if (loginResponse == null)
            {
                _logger.LogWarning("Failed to deserialize user session");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Check if token is expired
            if (loginResponse.ExpiresAtUtc.HasValue && loginResponse.ExpiresAtUtc.Value <= DateTime.UtcNow)
            {
                _logger.LogInformation("Token expired for user {Email}", loginResponse.Email);
                await MarkUserAsLoggedOutAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResponse.UserId.ToString()),
                new Claim(ClaimTypes.Email, loginResponse.Email),
                new Claim(ClaimTypes.Name, loginResponse.FullName),
                new Claim("TenantId", loginResponse.TenantId.ToString()),
                new Claim("access_token", loginResponse.Token ?? string.Empty)
            };

            // Add roles as claims
            foreach (var role in loginResponse.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Marks the user as authenticated and stores the session.
    /// </summary>
    public async Task MarkUserAsAuthenticatedAsync(LoginResponse loginResponse)
    {
        if (loginResponse == null)
        {
            throw new ArgumentNullException(nameof(loginResponse));
        }

        try
        {
            var sessionData = JsonSerializer.Serialize(loginResponse);
            await _protectedLocalStore.SetAsync(USER_SESSION_KEY, sessionData);

            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            _logger.LogInformation("User {Email} marked as authenticated. TenantId: {TenantId}", 
                loginResponse.Email, loginResponse.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as authenticated");
            throw;
        }
    }

    /// <summary>
    /// Marks the user as logged out and clears the session.
    /// </summary>
    public async Task MarkUserAsLoggedOutAsync()
    {
        try
        {
            await _protectedLocalStore.DeleteAsync(USER_SESSION_KEY);

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);

            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as logged out");
            throw;
        }
    }

    /// <summary>
    /// Gets the current user's tenant ID.
    /// </summary>
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

    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
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
