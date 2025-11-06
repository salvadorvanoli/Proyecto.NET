using System.Net.Http.Json;
using System.Text.Json;
using Application.AccessEvents.DTOs;
using Microsoft.Extensions.Logging;

namespace Mobile.Services;

/// <summary>
/// Implementación del servicio de API para eventos de acceso
/// </summary>
public class AccessEventApiService : IAccessEventApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccessEventApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AccessEventApiService(HttpClient httpClient, ILogger<AccessEventApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AccessEventResponse> CreateAccessEventAsync(CreateAccessEventRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("=== DIAGNÓSTICO CONEXIÓN ===");
            _logger.LogInformation("BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
            _logger.LogInformation("Endpoint: api/accessevents");
            _logger.LogInformation("Full URL: {FullUrl}", new Uri(_httpClient.BaseAddress!, "api/accessevents"));
            _logger.LogInformation("UserId: {UserId}, ControlPointId: {ControlPointId}", 
                request.UserId, request.ControlPointId);

            var response = await _httpClient.PostAsJsonAsync("api/accessevents", request, _jsonOptions, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<AccessEventResponse>(_jsonOptions, cancellationToken);
            
            if (result == null)
            {
                throw new InvalidOperationException("El servidor retornó una respuesta vacía");
            }

            _logger.LogInformation("Access event created successfully with ID {EventId}", result.Id);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error HTTP al crear evento de acceso");
            var detailedMessage = $"Error HTTP: {ex.Message}\n" +
                                  $"InnerException: {ex.InnerException?.Message ?? "N/A"}\n" +
                                  $"URL: {_httpClient.BaseAddress}api/accessevents";
            throw new InvalidOperationException(detailedMessage, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear evento de acceso");
            throw;
        }
    }

    public async Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting access events for user {UserId}", userId);

            var response = await _httpClient.GetAsync($"api/accessevents/user/{userId}", cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<List<AccessEventResponse>>(_jsonOptions, cancellationToken);
            
            return result ?? new List<AccessEventResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error HTTP al obtener eventos de acceso");
            throw new InvalidOperationException("No se pudo conectar con el servidor. Verifica tu conexión a internet.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener eventos de acceso");
            throw;
        }
    }
}
