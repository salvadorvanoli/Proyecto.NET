using Web.BackOffice.Services;
using Web.BackOffice.HealthChecks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

builder.Services.AddHttpClient<IRoleApiService, RoleApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

builder.Services.AddHttpClient<INewsApiService, NewsApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

builder.Services.AddHttpClient<ISpaceTypeApiService, SpaceTypeApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

builder.Services.AddHttpClient<ISpaceApiService, SpaceApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

builder.Services.AddHttpClient<IControlPointApiService, ControlPointApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddBackOfficeHealthChecks(builder.Configuration);

var app = builder.Build();

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
app.MapBackOfficeHealthChecks();
app.MapRazorPages();

app.Run();
