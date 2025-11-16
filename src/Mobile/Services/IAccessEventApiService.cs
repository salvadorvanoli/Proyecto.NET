using Application.AccessEvents.DTOs;
using Shared.DTOs.AccessEvents;

namespace Mobile.Services;

/// <summary>
/// Servicio para comunicarse con el backend API de eventos de acceso
/// </summary>
public interface IAccessEventApiService
{
    /// <summary>
    /// Crea un nuevo evento de acceso llamando al backend
    /// </summary>
    Task<AccessEventResponse> CreateAccessEventAsync(CreateAccessEventRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el historial de eventos de acceso de un usuario
    /// </summary>
    Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Valida si un usuario tiene acceso a un control point específico
    /// </summary>
    Task<AccessValidationResult> ValidateAccessAsync(int userId, int controlPointId, CancellationToken cancellationToken = default);
}
