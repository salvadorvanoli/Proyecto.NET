using Mobile.Models;

namespace Mobile.Data;

/// <summary>
/// Interface for local SQLite database operations.
/// </summary>
public interface ILocalDatabase
{
    /// <summary>
    /// Initializes the database and creates tables.
    /// </summary>
    Task InitializeDatabaseAsync();

    /// <summary>
    /// Saves an access event locally.
    /// </summary>
    Task<LocalAccessEvent> SaveAccessEventAsync(LocalAccessEvent accessEvent);

    /// <summary>
    /// Gets all unsynced access events.
    /// </summary>
    Task<List<LocalAccessEvent>> GetUnsyncedAccessEventsAsync();

    /// <summary>
    /// Gets all access events for a specific user.
    /// </summary>
    Task<List<LocalAccessEvent>> GetUserAccessEventsAsync(int userId, int limit = 50);

    /// <summary>
    /// Marks an access event as synced.
    /// </summary>
    Task MarkAccessEventAsSyncedAsync(int localId, int remoteId);

    /// <summary>
    /// Updates sync error for an access event.
    /// </summary>
    Task UpdateAccessEventSyncErrorAsync(int localId, string error);

    /// <summary>
    /// Gets the count of unsynced events.
    /// </summary>
    Task<int> GetUnsyncedCountAsync();

    /// <summary>
    /// Deletes old synced events (older than specified days).
    /// </summary>
    Task DeleteOldSyncedEventsAsync(int daysToKeep = 30);

    /// <summary>
    /// Saves access rules to local cache (replaces existing).
    /// </summary>
    Task SaveAccessRulesAsync(List<LocalAccessRule> rules);

    /// <summary>
    /// Gets cached access rules for a user at a specific control point.
    /// </summary>
    Task<List<LocalAccessRule>> GetAccessRulesForUserAsync(int userId, int controlPointId);

    /// <summary>
    /// Gets the count of cached access rules.
    /// </summary>
    Task<int> GetCachedRulesCountAsync();
}
