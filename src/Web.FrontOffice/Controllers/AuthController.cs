using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.DTOs.Auth;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthApiService _authApiService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthApiService authApiService, ILogger<AuthController> logger)
    {
        _authApiService = authApiService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authApiService.LoginAsync(request);

            if (response == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            if (string.IsNullOrEmpty(response.Token))
            {
                _logger.LogError("Login response missing token for user: {Email}", response.Email);
                return StatusCode(500, new { message = "Error en la respuesta del servidor" });
            }

            // Crear los claims del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new Claim(ClaimTypes.Email, response.Email),
                new Claim(ClaimTypes.Name, response.FullName),
                new Claim("TenantId", response.TenantId.ToString())
            };

            // Agregar roles como claims
            foreach (var role in response.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false, // Puedes parametrizar esto desde el request
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            // Almacenar el token JWT para usarlo en llamadas a la API
            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = response.Token
                },
                new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = response.ExpiresAtUtc?.ToString("O") ?? DateTime.UtcNow.AddHours(8).ToString("O")
                }
            });

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User {Email} logged in successfully. TenantId: {TenantId}", 
                response.Email, response.TenantId);

            // Retornar el token para que el componente Blazor lo use
            return Ok(new { 
                success = true, 
                token = response.Token,
                userId = response.UserId, 
                email = response.Email 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Ocurrió un error durante el inicio de sesión" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out successfully");
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "Ocurrió un error durante el cierre de sesión" });
        }
    }
}
