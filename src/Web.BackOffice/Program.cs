using Web.BackOffice.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Require authentication for all pages by default
    options.Conventions.AuthorizeFolder("/");

    // Allow anonymous access to Auth pages
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Logout");
    options.Conventions.AllowAnonymousToPage("/Auth/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
});

// Configure Authentication with Cookies
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

    // Default policy: require authenticated user
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add HttpContextAccessor for accessing user claims
builder.Services.AddHttpContextAccessor();

// Register TenantHeaderHandler
builder.Services.AddTransient<TenantHeaderHandler>();

// Configure HttpClient for User API with TenantHeaderHandler
builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/"); // URL de tu Web.Api
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

// Configure HttpClient for Role API with TenantHeaderHandler
builder.Services.AddHttpClient<IRoleApiService, RoleApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/"); // URL de tu Web.Api
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

// Configure HttpClient for News API with TenantHeaderHandler
builder.Services.AddHttpClient<INewsApiService, NewsApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/"); // URL de tu Web.Api
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TenantHeaderHandler>();

// Configure HttpClient for Auth API (no necesita TenantHeaderHandler porque es para login)
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/"); // URL de tu Web.Api
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore/swashbuckle.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
