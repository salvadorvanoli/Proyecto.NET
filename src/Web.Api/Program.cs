using Application;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Web.Api.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();
app.UseCors("AllowWebApps");
app.MapApiHealthChecks();
app.MapControllers();

app.Run();

// Método para asegurar que la base de datos existe
static async Task EnsureDatabaseExistsAsync(string? connectionString, ILogger logger)
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
static async Task DropAndRecreateDatabase(string? connectionString, ILogger logger)
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
