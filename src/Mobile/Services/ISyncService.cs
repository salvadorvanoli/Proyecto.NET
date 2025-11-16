using Mobile.Data;
using Shared.DTOs.AccessEvents;

namespace Mobile.Services;

/// <summary>
/// Service interface for synchronizing offline data with backend.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Gets the count of events pending synchronization.
    /// </summary>
    Task<int> GetPendingSyncCountAsync();

    /// <summary>
    /// Synchronizes all pending events with the backend.
    /// </summary>
    /// <returns>Number of events successfully synced.</returns>
    Task<int> SyncPendingEventsAsync();

    /// <summary>
    /// Checks if the device has network connectivity.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event fired when sync status changes.
    /// </summary>
    event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

    /// <summary>
    /// Event fired when connectivity changes.
    /// </summary>
    event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;
}

/// <summary>
/// Event args for sync status changes.
/// </summary>
public class SyncStatusChangedEventArgs : EventArgs
{
    public int TotalPending { get; set; }
    public int CurrentSync { get; set; }
    public int SuccessfulSync { get; set; }
    public int FailedSync { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Event args for connectivity changes.
/// </summary>
public class ConnectivityChangedEventArgs : EventArgs
{
    public bool IsConnected { get; set; }
}
