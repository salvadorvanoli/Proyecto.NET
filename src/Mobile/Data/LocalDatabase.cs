using SQLite;
using Microsoft.Extensions.Logging;
using Mobile.Models;

namespace Mobile.Data;

/// <summary>
/// SQLite database implementation for offline storage.
/// </summary>
public class LocalDatabase : ILocalDatabase
{
    private readonly ILogger<LocalDatabase> _logger;
    private SQLiteAsyncConnection? _database;

    public LocalDatabase(ILogger<LocalDatabase> logger)
    {
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        if (_database != null)
            return;

        try
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "accesscontrol.db");
            _logger.LogInformation("Initializing local database at: {Path}", databasePath);

            _database = new SQLiteAsyncConnection(databasePath);

            // Create tables
            await _database.CreateTableAsync<LocalAccessEvent>();
            await _database.CreateTableAsync<LocalAccessRule>();

            _logger.LogInformation("Local database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing local database");
            throw;
        }
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database == null)
        {
            await InitializeDatabaseAsync();
        }
        return _database!;
    }

    public async Task<LocalAccessEvent> SaveAccessEventAsync(LocalAccessEvent accessEvent)
    {
        try
        {
            var db = await GetDatabaseAsync();

            if (accessEvent.Id == 0)
            {
                accessEvent.CreatedAt = DateTime.UtcNow;
                await db.InsertAsync(accessEvent);
                _logger.LogInformation("Saved access event locally with ID {Id}", accessEvent.Id);
            }
            else
            {
                await db.UpdateAsync(accessEvent);
                _logger.LogInformation("Updated access event {Id}", accessEvent.Id);
            }

            return accessEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving access event locally");
            throw;
        }
    }

    public async Task<List<LocalAccessEvent>> GetUnsyncedAccessEventsAsync()
    {
        try
        {
            var db = await GetDatabaseAsync();
            var events = await db.Table<LocalAccessEvent>()
                .Where(e => !e.IsSynced)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} unsynced events", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unsynced events");
            throw;
        }
    }

    public async Task<List<LocalAccessEvent>> GetUserAccessEventsAsync(int userId, int limit = 50)
    {
        try
        {
            var db = await GetDatabaseAsync();
            var events = await db.Table<LocalAccessEvent>()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EventDateTime)
                .Take(limit)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} events for user {UserId}", events.Count, userId);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user access events");
            throw;
        }
    }

    public async Task MarkAccessEventAsSyncedAsync(int localId, int remoteId)
    {
        try
        {
            var db = await GetDatabaseAsync();
            var accessEvent = await db.Table<LocalAccessEvent>()
                .FirstOrDefaultAsync(e => e.Id == localId);

            if (accessEvent != null)
            {
                accessEvent.IsSynced = true;
                accessEvent.RemoteId = remoteId;
                accessEvent.SyncedAt = DateTime.UtcNow;
                accessEvent.SyncError = null;

                await db.UpdateAsync(accessEvent);
                _logger.LogInformation("Marked event {LocalId} as synced with remote ID {RemoteId}", localId, remoteId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking event as synced");
            throw;
        }
    }

    public async Task UpdateAccessEventSyncErrorAsync(int localId, string error)
    {
        try
        {
            var db = await GetDatabaseAsync();
            var accessEvent = await db.Table<LocalAccessEvent>()
                .FirstOrDefaultAsync(e => e.Id == localId);

            if (accessEvent != null)
            {
                accessEvent.SyncError = error;
                accessEvent.SyncRetryCount++;

                await db.UpdateAsync(accessEvent);
                _logger.LogWarning("Updated sync error for event {LocalId}: {Error}", localId, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sync error");
            throw;
        }
    }

    public async Task<int> GetUnsyncedCountAsync()
    {
        try
        {
            var db = await GetDatabaseAsync();
            return await db.Table<LocalAccessEvent>()
                .Where(e => !e.IsSynced)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unsynced count");
            return 0;
        }
    }

    public async Task DeleteOldSyncedEventsAsync(int daysToKeep = 30)
    {
        try
        {
            var db = await GetDatabaseAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var deletedCount = await db.ExecuteAsync(
                "DELETE FROM AccessEvents WHERE IsSynced = 1 AND SyncedAt < ?",
                cutoffDate);

            _logger.LogInformation("Deleted {Count} old synced events", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old synced events");
        }
    }

    #region Access Rules

    public async Task SaveAccessRulesAsync(List<LocalAccessRule> rules)
    {
        try
        {
            var db = await GetDatabaseAsync();
            
            // Clear existing rules
            await db.ExecuteAsync("DELETE FROM AccessRules");
            _logger.LogInformation("Cleared existing access rules");

            // Insert new rules
            await db.InsertAllAsync(rules);
            _logger.LogInformation("Saved {Count} access rules to local cache", rules.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving access rules");
            throw;
        }
    }

    public async Task<List<LocalAccessRule>> GetAccessRulesForUserAsync(int userId, int controlPointId)
    {
        try
        {
            var db = await GetDatabaseAsync();
            var rules = await db.Table<LocalAccessRule>()
                .Where(r => r.UserId == userId && r.ControlPointId == controlPointId && r.IsActive)
                .ToListAsync();

            _logger.LogInformation("Found {Count} cached rules for user {UserId} at control point {ControlPointId}", 
                rules.Count, userId, controlPointId);
            
            return rules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access rules");
            return new List<LocalAccessRule>();
        }
    }

    public async Task<int> GetCachedRulesCountAsync()
    {
        try
        {
            var db = await GetDatabaseAsync();
            return await db.Table<LocalAccessRule>().CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached rules count");
            return 0;
        }
    }

    #endregion
}
