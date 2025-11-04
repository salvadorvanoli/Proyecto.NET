using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for control point API service.
/// </summary>
public interface IControlPointApiService
{
    Task<ControlPointDto?> GetControlPointByIdAsync(int id);
    Task<IEnumerable<ControlPointDto>> GetControlPointsBySpaceAsync(int spaceId);
    Task<IEnumerable<ControlPointDto>> GetControlPointsByTenantAsync();
    Task<ControlPointDto?> CreateControlPointAsync(CreateControlPointDto dto);
    Task<bool> UpdateControlPointAsync(int id, UpdateControlPointDto dto);
    Task<bool> DeleteControlPointAsync(int id);
}
