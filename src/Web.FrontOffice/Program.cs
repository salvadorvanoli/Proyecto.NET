using Web.FrontOffice.Components;
using Web.FrontOffice.Services.Api;
using Web.FrontOffice.Services.Interfaces;
using Web.FrontOffice.Services;
using Web.FrontOffice.HealthChecks;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ========================================
// SEGURIDAD: Configurar Authentication & Authorization
// ========================================
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Registrar CustomAuthenticationStateProvider como Scoped
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrar JwtTokenHandler para agregar automáticamente el token JWT a las peticiones
builder.Services.AddScoped<JwtTokenHandler>();

// Add HttpClient factory for Blazor components
builder.Services.AddHttpClient();

// Configure API Base URL from configuration
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5236/";

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

    // Content Security Policy - ajustado para Blazor Server
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; img-src 'self' data: https:; font-src 'self' https://cdn.jsdelivr.net; connect-src 'self' ws: wss:; frame-ancestors 'self'");

    // Referrer Policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // Permissions Policy
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapFrontOfficeHealthChecks();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
