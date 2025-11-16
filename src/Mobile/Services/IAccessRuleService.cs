using Mobile.Models;
using Shared.DTOs.AccessEvents;

namespace Mobile.Services;

/// <summary>
/// Service for managing access rules (online and offline)
/// </summary>
public interface IMobileAccessRuleService
{
    /// <summary>
    /// Download access rules from server and cache locally
    /// </summary>
    Task<int> SyncAccessRulesAsync();

    /// <summary>
    /// Validate access using locally cached rules
    /// </summary>
    Task<AccessValidationResult> ValidateAccessOfflineAsync(int userId, int controlPointId, DateTime dateTime);

    /// <summary>
    /// Get all cached rules for a user
    /// </summary>
    Task<List<LocalAccessRule>> GetUserRulesAsync(int userId);

    /// <summary>
    /// Clear all cached rules
    /// </summary>
    Task ClearCacheAsync();

    /// <summary>
    /// Get last sync timestamp
    /// </summary>
    Task<DateTime?> GetLastSyncTimeAsync();
}
