using Application;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Web.Api.HealthChecks;
using Web.Api.Configuration;
using Web.Api.Middleware;
using Web.Api.Filters;
using Web.Api.Hubs;
using Web.Api.Services;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AspNetCoreRateLimit;
using System.Security.Claims;

// ========================================
// CONFIGURAR SERILOG (antes de crear builder)
// ========================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando ProyectoNet API...");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================
    // OBSERVABILIDAD: Configurar Serilog
    // ========================================
    builder.ConfigureSerilog();

    // ========================================
    // OBSERVABILIDAD: Configurar OpenTelemetry
    // ========================================
    builder.ConfigureOpenTelemetry();

    // Configurar timeouts para graceful shutdown
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        // Tiempo máximo para que las conexiones existentes terminen durante el shutdown
        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    });

    // Configurar opciones de host para graceful shutdown
    builder.Host.ConfigureHostOptions(hostOptions =>
    {
        // Tiempo que espera el host para que la aplicación se detenga antes de forzar el cierre
        hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(30);
    });

    builder.Services.AddControllers();
    builder.Services.AddSignalR();
    
    // Registrar TenantAuthorizationFilter como servicio para uso con atributos
    builder.Services.AddScoped<TenantAuthorizationFilter>();

    // ========================================
    // SEGURIDAD: Configurar JWT Authentication
    // ========================================
    var jwtSecret = builder.Configuration["Jwt:Secret"];
    if (!string.IsNullOrEmpty(jwtSecret))
    {
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProyectoNet",
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ProyectoNetClients",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            // Log authentication events for security monitoring
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                 ?? context.Principal?.FindFirst("sub")?.Value;
                    if (userId != null)
                    {
                        Log.Information("JWT validated for user {UserId}", userId);
                    }
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();
    }
    else
    {
        Log.Warning("⚠️ Jwt:Secret not configured. JWT authentication is disabled.");
    }

    // ========================================
    // SEGURIDAD: Configurar Rate Limiting
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
                Endpoint = "*",
                Period = "1s",
                Limit = 10 // Max 10 requests per second per IP (general)
            },
            new RateLimitRule
            {
                Endpoint = "*",
                Period = "1m",
                Limit = 200 // Max 200 requests per minute per IP (general)
            }
        };
    });
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowWebApps", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }
            else
            {
                // Soporta dos formatos:
                // 1. String separado por comas: CORS_ALLOWED_ORIGINS=http://localhost:4200,http://127.0.0.1:4200
                // 2. Array en appsettings.json: Cors:AllowedOrigins
                var corsOriginsString = builder.Configuration["CORS_ALLOWED_ORIGINS"];
                string[] allowedOrigins;

                if (!string.IsNullOrWhiteSpace(corsOriginsString))
                {
                    // Formato 1: String separado por comas
                    allowedOrigins = corsOriginsString
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(origin => !string.IsNullOrWhiteSpace(origin))
                        .ToArray();
                }
                else
                {
                    // Formato 2: Array en appsettings.json
                    allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                        ?? Array.Empty<string>();
                }

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
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Verificar si se debe recrear la base de datos
            var recreateDatabase = builder.Configuration.GetValue<bool>("RECREATE_DATABASE", false);
            if (recreateDatabase)
            {
                logger.LogWarning("RECREATE_DATABASE is enabled. Dropping and recreating database...");
                await DropAndRecreateDatabase(connectionString, logger);
            }
            else
            {
                // Crear la base de datos si no existe
                await EnsureDatabaseExistsAsync(connectionString, logger);
            }

            // Obtener el DbSeeder y ejecutar migraciones
            var dbSeeder = services.GetRequiredService<DbSeeder>();
            await dbSeeder.MigrateAsync();

            // Crear las tablas si no existen (para cuando no hay migraciones)
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            
            // Alternativamente, se puede usar MigrateAsync() en lugar de EnsureCreatedAsync()
            // await context.Database.MigrateAsync();
            
            logger.LogInformation("Database schema ensured.");

            // Ejecutar el seed si está habilitado
            var seedDatabase = builder.Configuration.GetValue<bool>("SEED_DATABASE", false);
            if (seedDatabase || app.Environment.IsDevelopment())
            {
                logger.LogInformation("Seeding database...");
                await DatabaseSeeder.SeedAsync(services, app.Environment);
            }

            logger.LogInformation("Database initialization completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            // No lanzar la excepción para que la aplicación pueda iniciar
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // ========================================
    // OBSERVABILIDAD: Middleware de CorrelationId
    // ========================================
    app.UseCorrelationId();

    // ========================================
    // SEGURIDAD: Middleware de Security Headers
    // ========================================
    app.Use(async (context, next) =>
    {
        // HSTS (HTTP Strict Transport Security)
        if (!app.Environment.IsDevelopment())
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        // Prevenir clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Prevenir MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // XSS Protection (aunque moderno está deprecado, muchos navegadores lo respetan)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Content Security Policy - ajustar según necesidades
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'");

        // Referrer Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions Policy (antes Feature-Policy)
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        await next();
    });

    // ========================================
    // SEGURIDAD: Rate Limiting Middleware
    // ========================================
    app.UseIpRateLimiting();

    // ========================================
    // OBSERVABILIDAD: Request Logging de Serilog
    // ========================================
    app.UseSerilogRequestLogging(options =>
    {
        // Personalizar el log de cada petición HTTP
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms | CorrelationId: {CorrelationId}";

        // Enriquecer con información adicional
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString() ?? "N/A");

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            }
        };
    });

    app.UseHttpsRedirection();
    
    // IMPORTANTE: CORS debe ir ANTES de MapHub para SignalR
    app.UseCors("AllowWebApps");
    
    // ========================================
    // SEGURIDAD: Authentication & Authorization
    // ========================================
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapApiHealthChecks();
    app.MapControllers();
    
    // Mapear SignalR Hub
    app.MapHub<NotificationHub>("/notificationHub");

    Log.Information("✅ API iniciado correctamente en {Environment}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error fatal al iniciar la aplicación");
    throw;
}
finally
{
    Log.Information("👋 Cerrando aplicación...");
    Log.CloseAndFlush();
}

// Método para asegurar que la base de datos existe
static async Task EnsureDatabaseExistsAsync(string? connectionString, Microsoft.Extensions.Logging.ILogger logger)
{
    if (string.IsNullOrEmpty(connectionString))
    {
        logger.LogWarning("Connection string is null or empty. Skipping database creation.");
        return;
    }

    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        // Conectar a la base de datos master para crear la BD
        builder.InitialCatalog = "master";
        var masterConnectionString = builder.ToString();

        logger.LogInformation("Checking if database '{DatabaseName}' exists...", databaseName);

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        // Verificar si la base de datos existe
        var checkDbCommand = connection.CreateCommand();
        checkDbCommand.CommandText = $"SELECT database_id FROM sys.databases WHERE Name = '{databaseName}'";
        var result = await checkDbCommand.ExecuteScalarAsync();

        if (result == null)
        {
            logger.LogInformation("Database '{DatabaseName}' does not exist. Creating...", databaseName);

            // Crear la base de datos
            var createDbCommand = connection.CreateCommand();
            createDbCommand.CommandText = $"CREATE DATABASE [{databaseName}]";
            await createDbCommand.ExecuteNonQueryAsync();

            logger.LogInformation("Database '{DatabaseName}' created successfully!", databaseName);
        }
        else
        {
            logger.LogInformation("Database '{DatabaseName}' already exists.", databaseName);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error ensuring database exists");
        throw;
    }
}

// Método para eliminar y recrear la base de datos
static async Task DropAndRecreateDatabase(string? connectionString, Microsoft.Extensions.Logging.ILogger logger)
{
    if (string.IsNullOrEmpty(connectionString))
    {
        logger.LogWarning("Connection string is null or empty. Skipping database recreation.");
        return;
    }

    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        // Conectar a la base de datos master para eliminar la BD
        builder.InitialCatalog = "master";
        var masterConnectionString = builder.ToString();

        logger.LogInformation("Dropping and recreating database '{DatabaseName}'...", databaseName);

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        // Eliminar la base de datos si existe
        var dropDbCommand = connection.CreateCommand();
        dropDbCommand.CommandText = $"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') BEGIN DROP DATABASE [{databaseName}] END";
        await dropDbCommand.ExecuteNonQueryAsync();

        logger.LogInformation("Database '{DatabaseName}' dropped successfully!", databaseName);

        // Crear la base de datos nuevamente
        var createDbCommand = connection.CreateCommand();
        createDbCommand.CommandText = $"CREATE DATABASE [{databaseName}]";
        await createDbCommand.ExecuteNonQueryAsync();

        logger.LogInformation("Database '{DatabaseName}' recreated successfully!", databaseName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error dropping and recreating database");
        throw;
    }
}
