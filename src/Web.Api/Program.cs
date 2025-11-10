using Application;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Web.Api.HealthChecks;
using Web.Api.Configuration;
using Web.Api.Middleware;
using Serilog;
using Microsoft.Extensions.Logging;

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

            // Ejecutar el seed si está habilitado
            var seedDatabase = builder.Configuration.GetValue<bool>("SEED_DATABASE", false);
            if (seedDatabase || app.Environment.IsDevelopment())
            {
                logger.LogInformation("Seeding database...");
                await dbSeeder.SeedAsync();
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
    app.UseCors("AllowWebApps");
    app.MapApiHealthChecks();
    app.MapControllers();

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
