using Web.FrontOffice.Components;
using Web.FrontOffice.Services.Api;
using Web.FrontOffice.Services.Interfaces;
using Web.FrontOffice.Services;
using Web.FrontOffice.HealthChecks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(); // Soporte para Razor Pages (Login/Logout)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(); // Agregar soporte para API controllers

// ========================================
// SEGURIDAD: Configurar Authentication & Authorization
// ========================================
// Configurar autenticación basada en cookies para Blazor Server
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = ".ProyectoNet.FrontOffice.Auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/frontoffice/";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// ========================================
// SEGURIDAD: Rate Limiting
// ========================================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1m",
            Limit = 5 // Max 5 login attempts per minute per IP
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/*",
            Period = "1m",
            Limit = 10
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "10s",
            Limit = 50 // Max 50 requests per 10 seconds (Blazor SignalR needs more)
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 200
        }
    };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Registrar CustomAuthenticationStateProvider como Scoped
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<Microsoft.AspNetCore.Identity.IdentityUser>>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrar JwtTokenHandler para agregar automáticamente el token JWT a las peticiones
builder.Services.AddScoped<JwtTokenHandler>();

// Add HttpClient factory for Blazor components
builder.Services.AddHttpClient();

// Configure API Base URL from environment variable (same as BackOffice)
var apiBaseUrl = builder.Configuration["API_BASE_URL"]
                 ?? Environment.GetEnvironmentVariable("API_BASE_URL")
                 ?? builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")
                 ?? "http://localhost:5236/";
Console.WriteLine($"Configuring FrontOffice to use API at: {apiBaseUrl}");

// Configure HttpClient for Auth API
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for Register API
builder.Services.AddHttpClient<IRegisterApiService, RegisterApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for News API
builder.Services.AddHttpClient<INewsApiService, NewsApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Configure HttpClient for Notification API
builder.Services.AddHttpClient<INotificationApiService, NotificationApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Configure HttpClient for User API
builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Configure HttpClient for Benefit API
builder.Services.AddHttpClient<IBenefitApiService, BenefitApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Configure HttpClient for AccessEvent API
builder.Services.AddHttpClient<IAccessEventApiService, AccessEventApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Configure HttpClient for Tenant API
builder.Services.AddHttpClient<ITenantApiService, TenantApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Agregar servicio de tema del tenant
builder.Services.AddScoped<TenantThemeService>();

// Agregar servicio de SignalR como Singleton para mantener la conexión persistente
builder.Services.AddSingleton<SignalRService>();

builder.Services.AddFrontOfficeHealthChecks(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// ========================================
// CONFIGURACIÓN: Forwarded Headers para ALB
// ========================================
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// ========================================
// CONFIGURACIÓN: Path Base para ALB (DEBE IR ANTES DE UseStaticFiles)
// ========================================
var pathBase = builder.Configuration["PathBase"] ?? Environment.GetEnvironmentVariable("PATH_BASE");
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    Console.WriteLine($"FrontOffice configured to use path base: {pathBase}");
}

// ========================================
// SEGURIDAD: Security Headers
// ========================================
app.Use(async (context, next) =>
{
    // HSTS (HTTP Strict Transport Security)
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    // Prevenir clickjacking
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");

    // Prevenir MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // XSS Protection
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // Content Security Policy - ajustado para Blazor Server y permitir source maps de CDN
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "connect-src 'self' ws: wss: https://cdn.jsdelivr.net; " +
        "frame-ancestors 'self'");

    // Referrer Policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // Permissions Policy
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    await next();
});

// ========================================
// SEGURIDAD: Rate Limiting Middleware
// ========================================
app.UseIpRateLimiting();

app.UseHttpsRedirection();

// Configurar archivos estáticos (UsePathBase ya está configurado arriba)
app.UseStaticFiles();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Mapear Razor Pages (Login/Logout)
app.MapControllers(); // Mapear API controllers
app.MapFrontOfficeHealthChecks();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
