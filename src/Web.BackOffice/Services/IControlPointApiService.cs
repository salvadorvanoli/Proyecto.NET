using Shared.DTOs.ControlPoints;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for control point API service.
/// </summary>
public interface IControlPointApiService
{
    Task<ControlPointResponse?> GetControlPointByIdAsync(int id);
    Task<IEnumerable<ControlPointResponse>> GetControlPointsBySpaceAsync(int spaceId);
    Task<IEnumerable<ControlPointResponse>> GetControlPointsByTenantAsync();
    Task<ControlPointResponse?> CreateControlPointAsync(ControlPointRequest dto);
    Task<bool> UpdateControlPointAsync(int id, ControlPointRequest dto);
    Task<bool> DeleteControlPointAsync(int id);
}
