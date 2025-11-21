using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

/// <summary>
/// Handles database migrations for the application.
/// For seeding initial data, use DatabaseSeeder instead.
/// </summary>
public class MigrationRunner
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MigrationRunner> _logger;

    public MigrationRunner(ApplicationDbContext context, ILogger<MigrationRunner> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Applies pending migrations to the database.
    /// </summary>
    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Checking for pending migrations...");

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("Database is up to date. No migrations needed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}
