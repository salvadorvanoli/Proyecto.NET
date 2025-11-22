using Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthApiService _authApiService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IAuthApiService authApiService, ILogger<LoginModel> logger)
    {
        _authApiService = authApiService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var loginRequest = new LoginRequest
            {
                Email = Input.Email,
                Password = Input.Password
            };

            var response = await _authApiService.LoginAsync(loginRequest);

            if (response == null)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return Page();
            }

            // Verificar que el usuario tenga el rol de AdministradorBackoffice
            if (!response.Roles.Contains("AdministradorBackoffice"))
            {
                ModelState.AddModelError(string.Empty, "No tiene permisos para acceder al BackOffice.");
                _logger.LogWarning("User {Email} attempted to login without AdministradorBackoffice role", Input.Email);
                return Page();
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
                IsPersistent = Input.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(Input.RememberMe ? 24 : 8)
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

            _logger.LogInformation("User {Email} logged in successfully. TenantId: {TenantId}", response.Email, response.TenantId);

            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError(string.Empty, "Ocurrió un error durante el inicio de sesión.");
            return Page();
        }
    }
}

