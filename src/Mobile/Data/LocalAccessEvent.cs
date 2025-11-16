using SQLite;

namespace Mobile.Data;

/// <summary>
/// Local SQLite entity for access events (offline storage).
/// </summary>
[Table("AccessEvents")]
public class LocalAccessEvent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Remote ID from backend (null if not synced yet).
    /// </summary>
    public int? RemoteId { get; set; }

    /// <summary>
    /// User ID who attempted access.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Control point ID where access was attempted.
    /// </summary>
    public int ControlPointId { get; set; }

    /// <summary>
    /// Event date and time (UTC).
    /// </summary>
    public DateTime EventDateTime { get; set; }

    /// <summary>
    /// Access result: "Granted" or "Denied".
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the access decision.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// User's name at the time of event.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Control point name at the time of event.
    /// </summary>
    public string ControlPointName { get; set; } = string.Empty;

    /// <summary>
    /// NFC tag ID that was scanned.
    /// </summary>
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this event has been synchronized with the backend.
    /// </summary>
    public bool IsSynced { get; set; }

    /// <summary>
    /// Timestamp when the event was created locally.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the event was synced to backend (null if not synced).
    /// </summary>
    public DateTime? SyncedAt { get; set; }

    /// <summary>
    /// Error message if sync failed.
    /// </summary>
    public string? SyncError { get; set; }

    /// <summary>
    /// Number of sync retry attempts.
    /// </summary>
    public int SyncRetryCount { get; set; }
}
