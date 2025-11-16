using SQLite;

namespace Mobile.Models;

/// <summary>
/// Local cache of access rules for offline validation
/// </summary>
[Table("AccessRules")]
public class LocalAccessRule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// User ID that this rule applies to
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Control point ID where access is allowed
    /// </summary>
    public int ControlPointId { get; set; }

    /// <summary>
    /// Space ID (optional, for additional context)
    /// </summary>
    public int? SpaceId { get; set; }

    /// <summary>
    /// Allowed days of week (comma-separated: 1,2,3,4,5)
    /// </summary>
    public string AllowedDays { get; set; } = string.Empty;

    /// <summary>
    /// Start time in format HH:mm
    /// </summary>
    public string StartTime { get; set; } = "00:00";

    /// <summary>
    /// End time in format HH:mm
    /// </summary>
    public string EndTime { get; set; } = "23:59";

    /// <summary>
    /// Whether this rule is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this rule was last synced from server
    /// </summary>
    public DateTime LastSyncedAt { get; set; }
}
