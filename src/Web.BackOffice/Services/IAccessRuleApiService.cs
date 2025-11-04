using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for access rule API service.
/// </summary>
public interface IAccessRuleApiService
{
    Task<AccessRuleDto?> GetAccessRuleByIdAsync(int id);
    Task<IEnumerable<AccessRuleDto>> GetAccessRulesByTenantAsync();
    Task<IEnumerable<AccessRuleDto>> GetAccessRulesByControlPointAsync(int controlPointId);
    Task<IEnumerable<AccessRuleDto>> GetAccessRulesByRoleAsync(int roleId);
    Task<AccessRuleDto?> CreateAccessRuleAsync(CreateAccessRuleDto dto);
    Task<bool> UpdateAccessRuleAsync(int id, UpdateAccessRuleDto dto);
    Task<bool> DeleteAccessRuleAsync(int id);
}
