using Shared.DTOs.AccessRules;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for access rule API service.
/// </summary>
public interface IAccessRuleApiService
{
    Task<AccessRuleResponse?> GetAccessRuleByIdAsync(int id);
    Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByTenantAsync();
    Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByControlPointAsync(int controlPointId);
    Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByRoleAsync(int roleId);
    Task<AccessRuleResponse?> CreateAccessRuleAsync(AccessRuleRequest dto);
    Task<bool> UpdateAccessRuleAsync(int id, AccessRuleRequest dto);
    Task<bool> DeleteAccessRuleAsync(int id);
}
