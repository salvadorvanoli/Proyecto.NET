using Shared.DTOs.AccessRules;

namespace Application.AccessRules;

/// <summary>
/// Service interface for access rule management.
/// </summary>
public interface IAccessRuleService
{
    Task<AccessRuleResponse?> GetAccessRuleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByTenantAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByControlPointAsync(int controlPointId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<AccessRuleResponse> CreateAccessRuleAsync(AccessRuleRequest request, CancellationToken cancellationToken = default);
    Task<AccessRuleResponse> UpdateAccessRuleAsync(int id, AccessRuleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAccessRuleAsync(int id, CancellationToken cancellationToken = default);
}
