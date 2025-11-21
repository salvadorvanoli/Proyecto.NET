using Mobile.Data;
using Mobile.Models;
using System.Net.Http.Json;

namespace Mobile.Services;

// Modelo interno para deserializar la respuesta del backend
internal class AccessEventResponseBackend
{
    public int Id { get; set; }
    public DateTime EventDateTime { get; set; }
    public string Result { get; set; } = string.Empty;
    public ControlPointResponseBackend ControlPoint { get; set; } = null!;
    public int UserId { get; set; }
}

internal class ControlPointResponseBackend
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SpaceResponseBackend Space { get; set; } = null!;
}

internal class SpaceResponseBackend
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AccessEventService : IAccessEventService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private readonly ILocalDatabase _localDatabase;

    public AccessEventService(
        IHttpClientFactory httpClientFactory, 
        IAuthService authService,
        ILocalDatabase localDatabase)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _localDatabase = localDatabase;
    }

    public async Task<List<AccessEventDto>> GetMyAccessEventsAsync(int skip = 0, int take = 20)
    {
        System.Diagnostics.Debug.WriteLine($"[AccessEventService] GetMyAccessEventsAsync called - skip: {skip}, take: {take}");
        
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            System.Diagnostics.Debug.WriteLine("[AccessEventService] No current user found");
            return new List<AccessEventDto>();
        }

        System.Diagnostics.Debug.WriteLine($"[AccessEventService] Current user: {currentUser.Email}");

        try
        {
            // Intentar obtener del servidor
            var httpClient = _httpClientFactory.CreateClient("AccessEventClient");
            System.Diagnostics.Debug.WriteLine($"[AccessEventService] HttpClient BaseAddress: {httpClient.BaseAddress}");
            
            // Agregar token de autorización
            if (!string.IsNullOrEmpty(currentUser.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.Token);
                System.Diagnostics.Debug.WriteLine("[AccessEventService] Authorization token added");
            }
            
            var url = $"/api/access-events/my-events?skip={skip}&take={take}";
            System.Diagnostics.Debug.WriteLine($"[AccessEventService] Requesting: {url}");
            
            var response = await httpClient.GetAsync(url);
            System.Diagnostics.Debug.WriteLine($"[AccessEventService] Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[AccessEventService] Response content: {content}");
                
                var backendEvents = await response.Content.ReadFromJsonAsync<List<AccessEventResponseBackend>>();
                if (backendEvents != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AccessEventService] Deserialized {backendEvents.Count} events from backend");
                    
                    // Guardar eventos del servidor en BD local para acceso offline
                    foreach (var backendEvent in backendEvents)
                    {
                        // Asegurar que la fecha del backend se interprete como UTC
                        var utcTimestamp = backendEvent.EventDateTime.Kind == DateTimeKind.Utc
                            ? backendEvent.EventDateTime
                            : DateTime.SpecifyKind(backendEvent.EventDateTime, DateTimeKind.Utc);
                        
                        var localEvent = new LocalAccessEvent
                        {
                            UserId = backendEvent.UserId,
                            ControlPointId = backendEvent.ControlPoint?.Id ?? 0,
                            ControlPointName = backendEvent.ControlPoint?.Name ?? "Desconocido",
                            SpaceName = backendEvent.ControlPoint?.Space?.Name ?? "Desconocido",
                            Timestamp = utcTimestamp,
                            WasGranted = backendEvent.Result == "Granted",
                            DenialReason = backendEvent.Result != "Granted" ? backendEvent.Result : null,
                            IsSynced = true // Eventos del servidor ya están sincronizados
                        };
                        
                        // Solo guardar si no existe ya (evitar duplicados)
                        var existingEvents = await _localDatabase.GetAccessEventsAsync(currentUser.UserId, 0, int.MaxValue);
                        if (!existingEvents.Any(e => 
                            e.ControlPointId == localEvent.ControlPointId && 
                            Math.Abs((e.Timestamp - localEvent.Timestamp).TotalSeconds) < 2))
                        {
                            await _localDatabase.SaveAccessEventAsync(localEvent);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"[AccessEventService] Cached {backendEvents.Count} events in local database");
                    
                    var mappedEvents = backendEvents.Select(e => 
                    {
                        // Asegurar que la fecha del backend se interprete como UTC
                        var utcTimestamp = e.EventDateTime.Kind == DateTimeKind.Utc
                            ? e.EventDateTime
                            : DateTime.SpecifyKind(e.EventDateTime, DateTimeKind.Utc);
                        
                        return new AccessEventDto
                        {
                            Id = e.Id,
                            UserId = e.UserId,
                            ControlPointId = e.ControlPoint?.Id ?? 0,
                            ControlPointName = e.ControlPoint?.Name ?? "Desconocido",
                            SpaceName = e.ControlPoint?.Space?.Name ?? "Desconocido",
                            Timestamp = utcTimestamp,
                            WasGranted = e.Result == "Granted",
                            DenialReason = e.Result != "Granted" ? e.Result : null
                        };
                    }).ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"[AccessEventService] Returning {mappedEvents.Count} events from server");
                    return mappedEvents;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[AccessEventService] Error response: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AccessEventService] Exception getting events from server: {ex.GetType().Name} - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AccessEventService] Stack trace: {ex.StackTrace}");
        }

        // Si falla, obtener de base de datos local
        System.Diagnostics.Debug.WriteLine("[AccessEventService] Falling back to local database");
        var localEvents = await _localDatabase.GetAccessEventsAsync(currentUser.UserId, skip, take);
        System.Diagnostics.Debug.WriteLine($"[AccessEventService] Found {localEvents.Count} events in local database");
        
        return localEvents.Select(e => new AccessEventDto
        {
            Id = e.Id,
            UserId = e.UserId,
            ControlPointId = e.ControlPointId,
            ControlPointName = e.ControlPointName,
            SpaceName = e.SpaceName,
            Timestamp = e.Timestamp,
            WasGranted = e.WasGranted,
            DenialReason = e.DenialReason
        }).ToList();
    }

    public async Task<int> GetTotalAccessEventsCountAsync()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
            return 0;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("AccessEventClient");
            
            // Agregar token de autorización
            if (!string.IsNullOrEmpty(currentUser.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.Token);
            }
            
            var response = await httpClient.GetAsync("/api/access-events/my-events/count");
            
            if (response.IsSuccessStatusCode)
            {
                var count = await response.Content.ReadFromJsonAsync<int>();
                return count;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting count from server: {ex.Message}");
        }

        // Si falla, obtener count de base de datos local
        return await _localDatabase.GetTotalEventsCountAsync(currentUser.UserId);
    }

    public async Task<bool> SaveAccessEventAsync(AccessEventDto accessEvent)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
            return false;

        // Guardar localmente primero
        var localEvent = new LocalAccessEvent
        {
            UserId = currentUser.UserId,
            ControlPointId = accessEvent.ControlPointId,
            ControlPointName = accessEvent.ControlPointName,
            SpaceName = accessEvent.SpaceName,
            Timestamp = accessEvent.Timestamp,
            WasGranted = accessEvent.WasGranted,
            DenialReason = accessEvent.DenialReason,
            IsSynced = false
        };

        await _localDatabase.SaveAccessEventAsync(localEvent);

        // Intentar enviar al servidor si hay conexión
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AccessEventClient");
            
            // Agregar token de autorización
            if (!string.IsNullOrEmpty(currentUser.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.Token);
            }
            
            var response = await httpClient.PostAsJsonAsync("/api/access-events", accessEvent);
            
            if (response.IsSuccessStatusCode)
            {
                // Marcar como sincronizado
                await _localDatabase.MarkEventAsSyncedAsync(localEvent.Id);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Could not sync event to server: {ex.Message}");
            // No es error crítico, el evento se guardó localmente
        }

        return true; // Retornamos true porque se guardó localmente
    }
}
