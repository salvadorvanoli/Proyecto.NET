using Web.BackOffice.Services;
using Web.BackOffice.HealthChecks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configurar timeouts para graceful shutdown
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// Configurar opciones de host para graceful shutdown
builder.Host.ConfigureHostOptions(hostOptions =>
{
    hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// Configurar Data Protection para compartir cookies entre múltiples instancias
// Esto es CRÍTICO para balanceo de carga con autenticación basada en cookies
var dataProtectionPath = builder.Configuration["DataProtection:KeyPath"]
    ?? Environment.GetEnvironmentVariable("DataProtection__KeyPath")
    ?? Path.Combine(Path.GetTempPath(), "ProyectoNet-DataProtection-Keys");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("ProyectoNet.BackOffice")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Logout");
    options.Conventions.AllowAnonymousToPage("/Auth/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
});

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
        options.Cookie.Name = ".ProyectoNet.BackOffice.Auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministradorBackoffice", policy =>
        policy.RequireRole("AdministradorBackoffice"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TenantHeaderHandler>();
builder.Services.AddTransient<JwtTokenHandler>();

// Obtener la URL del API desde variable de entorno
var apiBaseUrl = builder.Configuration["API_BASE_URL"] 
                 ?? Environment.GetEnvironmentVariable("API_BASE_URL") 
                 ?? "http://localhost:5000";
Console.WriteLine($"Configuring BackOffice to use API at: {apiBaseUrl}");

builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<IRoleApiService, RoleApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<INewsApiService, NewsApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<ISpaceTypeApiService, SpaceTypeApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<ISpaceApiService, SpaceApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<IControlPointApiService, ControlPointApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<IAccessRuleApiService, AccessRuleApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<IBenefitTypeApiService, BenefitTypeApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<IBenefitApiService, BenefitApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddBackOfficeHealthChecks(builder.Configuration);

var app = builder.Build();

// Registrar eventos de ciclo de vida para graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("BackOffice application is stopping. Waiting for requests to complete...");
});

lifetime.ApplicationStopped.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("BackOffice application stopped successfully.");
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Mapear health checks DESPUÉS de UseAuthorization pero con AllowAnonymous
app.MapBackOfficeHealthChecks();
app.MapRazorPages();

app.Run();
