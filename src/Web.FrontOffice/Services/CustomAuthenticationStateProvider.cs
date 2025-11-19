using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Web.FrontOffice.Services;

/// <summary>
/// AuthenticationStateProvider que revalida el estado de autenticación basado en cookies HTTP.
/// Compatible con Razor Pages + Blazor Server.
/// </summary>
public class RevalidatingIdentityAuthenticationStateProvider<TUser>
    : RevalidatingServerAuthenticationStateProvider where TUser : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RevalidatingIdentityAuthenticationStateProvider<TUser>> _logger;

    public RevalidatingIdentityAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = loggerFactory.CreateLogger<RevalidatingIdentityAuthenticationStateProvider<TUser>>();
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Obtener el usuario actual
        var user = authenticationState.User;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Validar que el token no haya expirado
        var expirationClaim = user.FindFirst("exp");
        if (expirationClaim != null && long.TryParse(expirationClaim.Value, out var exp))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
            if (expirationTime < DateTimeOffset.UtcNow)
            {
                _logger.LogInformation("Token expired, logging out user");
                return false;
            }
        }

        return true;
    }
}