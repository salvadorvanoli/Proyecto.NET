using Application.AccessEvents.DTOs;
using Microsoft.Extensions.Logging;
using Mobile.Data;

namespace Mobile.Services;

/// <summary>
/// Service for synchronizing offline data with backend.
/// </summary>
public class SyncService : ISyncService
{
    private readonly ILocalDatabase _localDatabase;
    private readonly IAccessEventApiService _accessEventService;
    private readonly ILogger<SyncService> _logger;

    public event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;
    public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

    public SyncService(
        ILocalDatabase localDatabase,
        IAccessEventApiService accessEventService,
        ILogger<SyncService> logger)
    {
        _localDatabase = localDatabase;
        _accessEventService = accessEventService;
        _logger = logger;

        // Subscribe to connectivity changes
        Connectivity.ConnectivityChanged += OnConnectivityChangedHandler;
    }

    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public async Task<int> GetPendingSyncCountAsync()
    {
        return await _localDatabase.GetUnsyncedCountAsync();
    }

    public async Task<int> SyncPendingEventsAsync()
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot sync: No internet connection");
            return 0;
        }

        try
        {
            var unsyncedEvents = await _localDatabase.GetUnsyncedAccessEventsAsync();
            
            if (unsyncedEvents.Count == 0)
            {
                _logger.LogInformation("No events to sync");
                return 0;
            }

            _logger.LogInformation("Starting sync of {Count} events", unsyncedEvents.Count);

            int successCount = 0;
            int failCount = 0;

            for (int i = 0; i < unsyncedEvents.Count; i++)
            {
                var localEvent = unsyncedEvents[i];

                try
                {
                    // Notify progress
                    SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
                    {
                        TotalPending = unsyncedEvents.Count,
                        CurrentSync = i + 1,
                        SuccessfulSync = successCount,
                        FailedSync = failCount,
                        IsCompleted = false
                    });

                    // Create request from local event
                    var request = new CreateAccessEventRequest
                    {
                        UserId = localEvent.UserId,
                        ControlPointId = localEvent.ControlPointId,
                        EventDateTime = localEvent.EventDateTime,
                        Result = localEvent.Result
                    };

                    // Send to backend
                    var response = await _accessEventService.CreateAccessEventAsync(request);

                    // Mark as synced
                    await _localDatabase.MarkAccessEventAsSyncedAsync(localEvent.Id, response.Id);

                    successCount++;
                    _logger.LogInformation("Synced event {LocalId} -> {RemoteId}", localEvent.Id, response.Id);
                }
                catch (Exception ex)
                {
                    failCount++;
                    var errorMessage = $"Sync failed: {ex.Message}";
                    await _localDatabase.UpdateAccessEventSyncErrorAsync(localEvent.Id, errorMessage);
                    _logger.LogError(ex, "Failed to sync event {LocalId}", localEvent.Id);
                }
            }

            // Notify completion
            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
            {
                TotalPending = unsyncedEvents.Count,
                CurrentSync = unsyncedEvents.Count,
                SuccessfulSync = successCount,
                FailedSync = failCount,
                IsCompleted = true
            });

            _logger.LogInformation("Sync completed: {Success} successful, {Failed} failed", successCount, failCount);

            // Clean up old synced events
            await _localDatabase.DeleteOldSyncedEventsAsync(30);

            return successCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sync");
            throw;
        }
    }

    private async void OnConnectivityChangedHandler(object? sender, Microsoft.Maui.Networking.ConnectivityChangedEventArgs e)
    {
        var isConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
        
        _logger.LogInformation("Connectivity changed: {Status}", isConnected ? "Connected" : "Disconnected");

        ConnectivityChanged?.Invoke(this, new Services.ConnectivityChangedEventArgs
        {
            IsConnected = isConnected
        });

        // Auto-sync when connectivity is restored
        if (isConnected)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000); // Wait 2 seconds before syncing
                try
                {
                    await SyncPendingEventsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Auto-sync failed");
                }
            });
        }
    }
}
