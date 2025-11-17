using Application;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Web.Api.HealthChecks;
using Web.Api.Hubs;
using Web.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApps", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                    "http://localhost:5001",
                    "https://localhost:5001",
                    "http://localhost:5002",
                    "https://localhost:5002",
                    "http://localhost:5000",
                    "https://localhost:5000"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                                ?? Array.Empty<string>();
            
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Override the default NotificationHubService with SignalR implementation
// Usar Singleton para que sea compatible con IHubContext que es Singleton
builder.Services.AddSingleton<INotificationHubService, SignalRNotificationHubService>();

builder.Services.AddApiHealthChecks(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        
        var seedDatabase = builder.Configuration.GetValue<bool>("SEED_DATABASE", false);
        if (seedDatabase || app.Environment.IsDevelopment())
        {
            await DatabaseSeeder.SeedAsync(services, app.Environment);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANTE: CORS debe ir ANTES de MapHub
app.UseCors("AllowWebApps");

app.MapApiHealthChecks();
app.MapControllers();

// Mapear SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
