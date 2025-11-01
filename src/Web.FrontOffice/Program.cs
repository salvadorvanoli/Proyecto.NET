using Web.FrontOffice.Components;
using Web.FrontOffice.Services.Api;
using Web.FrontOffice.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Configure HttpClient for User API
// TODO: Agregar HttpMessageHandler para incluir X-Tenant-Id automáticamente desde las claims del usuario
builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for Benefit API
// TODO: Agregar HttpMessageHandler para incluir X-Tenant-Id automáticamente desde las claims del usuario
builder.Services.AddHttpClient<IBenefitApiService, BenefitApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for AccessEvent API
// TODO: Agregar HttpMessageHandler para incluir X-Tenant-Id automáticamente desde las claims del usuario
builder.Services.AddHttpClient<IAccessEventApiService, AccessEventApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
