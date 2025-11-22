using Mobile.Data;
using Mobile.Models;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.Messaging;
using Mobile.Messages;

namespace Mobile.Services;

public class SyncService : ISyncService
{
    private readonly ILocalDatabase _localDatabase;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public SyncService(
        ILocalDatabase localDatabase,
        IHttpClientFactory httpClientFactory,
        IAuthService authService,
        IUserService userService)
    {
        _localDatabase = localDatabase;
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _userService = userService;
    }

    public async Task SyncPendingEventsAsync()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
            return;

        try
        {
            var unsyncedEvents = await _localDatabase.GetUnsyncedEventsAsync(currentUser.UserId);
            
            if (unsyncedEvents.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No pending events to sync");
                return;
            }

            var httpClient = _httpClientFactory.CreateClient("AccessEventClient");
            
            // Agregar token de autorización
            if (!string.IsNullOrEmpty(currentUser.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.Token);
            }
            
            var syncedCount = 0;

            foreach (var localEvent in unsyncedEvents)
            {
                try
                {
                    var dto = new AccessEventDto
                    {
                        UserId = localEvent.UserId,
                        ControlPointId = localEvent.ControlPointId,
                        ControlPointName = localEvent.ControlPointName,
                        SpaceName = localEvent.SpaceName,
                        Timestamp = localEvent.Timestamp,
                        WasGranted = localEvent.WasGranted,
                        DenialReason = localEvent.DenialReason
                    };

                    var response = await httpClient.PostAsJsonAsync("/api/access-events", dto);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        await _localDatabase.MarkEventAsSyncedAsync(localEvent.Id);
                        syncedCount++;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to sync event {localEvent.Id}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"✅ Synced {syncedCount} of {unsyncedEvents.Count} pending events");
            
            // Notificar que se completó la sincronización
            if (syncedCount > 0)
            {
                WeakReferenceMessenger.Default.Send(new EventsSyncedMessage { SyncedCount = syncedCount });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sync error: {ex.Message}");
        }
    }

    public async Task<bool> CheckUserStatusAsync()
    {
        // Solo validar si hay conectividad
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            return true; // No validar offline, mantener sesión
        }

        try
        {
            return await _userService.IsUserActiveAsync();
        }
        catch
        {
            // Si hay error al validar, mantener sesión (beneficio de la duda)
            return true;
        }
    }
}
