using Microsoft.Extensions.Logging;
using Mobile.Data;
using Mobile.Models;
using Shared.DTOs;
using Shared.DTOs.AccessEvents;

namespace Mobile.Services;

/// <summary>
/// Service for managing access rules (download, cache, offline validation)
/// </summary>
public class AccessRuleService : IMobileAccessRuleService
{
    private readonly AccessRuleApiService _apiService;
    private readonly ILocalDatabase _localDatabase;
    private readonly ILogger<AccessRuleService> _logger;

    public AccessRuleService(
        AccessRuleApiService apiService,
        ILocalDatabase localDatabase,
        ILogger<AccessRuleService> logger)
    {
        _apiService = apiService;
        _localDatabase = localDatabase;
        _logger = logger;
    }

    public async Task<int> SyncAccessRulesAsync()
    {
        try
        {
            _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _logger.LogInformation("🔄 Syncing access rules from backend...");

            // Download rules from backend
            var rulesDto = await _apiService.GetAccessRulesAsync();
            
            // Convert to local model
            var localRules = rulesDto.Select(dto => new LocalAccessRule
            {
                UserId = dto.UserId,
                ControlPointId = dto.ControlPointId,
                SpaceId = dto.SpaceId,
                AllowedDays = dto.AllowedDays,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = dto.IsActive,
                LastSyncedAt = DateTime.UtcNow
            }).ToList();

            // Save to local cache
            await _localDatabase.SaveAccessRulesAsync(localRules);

            _logger.LogInformation("✅ Synced {Count} access rules successfully", localRules.Count);
            _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            return localRules.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error syncing access rules");
            throw;
        }
    }

    public async Task<AccessValidationResult> ValidateAccessOfflineAsync(int userId, int controlPointId, DateTime dateTime)
    {
        try
        {
            _logger.LogInformation("🔍 Validating access OFFLINE - User: {UserId}, ControlPoint: {ControlPointId}", 
                userId, controlPointId);

            // Get cached rules for this user and control point
            var rules = await _localDatabase.GetAccessRulesForUserAsync(userId, controlPointId);

            if (!rules.Any())
            {
                _logger.LogWarning("❌ No cached rules found for user {UserId} at control point {ControlPointId}", 
                    userId, controlPointId);
                
                return new AccessValidationResult
                {
                    IsGranted = false,
                    Reason = "Sin reglas de acceso (Modo Offline)",
                    UserName = $"Usuario {userId}",
                    ControlPointName = $"Punto de Control {controlPointId}"
                };
            }

            // Check if any rule allows access at current time
            var dayOfWeek = (int)dateTime.DayOfWeek; // 0=Sunday, 6=Saturday
            var currentTime = dateTime.TimeOfDay;

            foreach (var rule in rules)
            {
                // Check day of week
                var allowedDays = rule.AllowedDays.Split(',').Select(int.Parse).ToList();
                if (!allowedDays.Contains(dayOfWeek))
                {
                    _logger.LogDebug("Rule rejected: day {Day} not allowed (allowed: {AllowedDays})", 
                        dayOfWeek, rule.AllowedDays);
                    continue;
                }

                // Check time range
                if (TimeSpan.TryParse(rule.StartTime, out var startTime) &&
                    TimeSpan.TryParse(rule.EndTime, out var endTime))
                {
                    if (currentTime >= startTime && currentTime <= endTime)
                    {
                        _logger.LogInformation("✅ Access GRANTED by cached rule (offline)");
                        
                        return new AccessValidationResult
                        {
                            IsGranted = true,
                            Reason = $"Acceso permitido (Modo Offline) - Horario: {rule.StartTime}-{rule.EndTime}",
                            UserName = $"Usuario {userId}",
                            ControlPointName = $"Punto de Control {controlPointId}"
                        };
                    }
                    else
                    {
                        _logger.LogDebug("Rule rejected: time {CurrentTime} outside range {StartTime}-{EndTime}", 
                            currentTime, rule.StartTime, rule.EndTime);
                    }
                }
            }

            _logger.LogWarning("❌ Access DENIED - No matching rules (offline)");
            
            return new AccessValidationResult
            {
                IsGranted = false,
                Reason = "Fuera de horario permitido (Modo Offline)",
                UserName = $"Usuario {userId}",
                ControlPointName = $"Punto de Control {controlPointId}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access offline");
            throw;
        }
    }

    public async Task<List<LocalAccessRule>> GetUserRulesAsync(int userId)
    {
        var db = _localDatabase;
        // This would need a method to get all rules for a user across all control points
        // For now, returning empty as we query by userId + controlPointId
        return new List<LocalAccessRule>();
    }

    public async Task ClearCacheAsync()
    {
        await _localDatabase.SaveAccessRulesAsync(new List<LocalAccessRule>());
        _logger.LogInformation("Cleared access rules cache");
    }

    public async Task<DateTime?> GetLastSyncTimeAsync()
    {
        await Task.CompletedTask;
        // Could store this in Preferences or get from database
        var timestamp = Preferences.Get("LastRuleSyncTime", string.Empty);
        return string.IsNullOrEmpty(timestamp) ? null : DateTime.Parse(timestamp);
    }
}
