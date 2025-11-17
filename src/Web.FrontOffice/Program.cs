using Web.FrontOffice.Components;
using Web.FrontOffice.Services.Api;
using Web.FrontOffice.Services.Interfaces;
using Web.FrontOffice.Services;
using Web.FrontOffice.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// TODO: Configurar autenticación cuando se implemente el sistema de login
// builder.Services.AddAuthentication(...)
// builder.Services.AddAuthorization();
// builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
// builder.Services.AddCascadingAuthenticationState();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

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
});

// Configure HttpClient for Notification API
builder.Services.AddHttpClient<INotificationApiService, NotificationApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for User API
builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for Benefit API
builder.Services.AddHttpClient<IBenefitApiService, BenefitApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for AccessEvent API
builder.Services.AddHttpClient<IAccessEventApiService, AccessEventApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for Tenant API
builder.Services.AddHttpClient<ITenantApiService, TenantApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapFrontOfficeHealthChecks();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
