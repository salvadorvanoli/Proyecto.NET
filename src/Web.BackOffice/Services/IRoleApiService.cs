using Shared.DTOs.Roles;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing roles through the API.
/// </summary>
public interface IRoleApiService
{
    Task<IEnumerable<RoleResponse>> GetAllRolesAsync();
    Task<RoleResponse?> GetRoleByIdAsync(int id);
    Task<RoleResponse> CreateRoleAsync(RoleRequest createRoleDto);
    Task<RoleResponse> UpdateRoleAsync(int id, RoleRequest updateRoleDto);
    Task<bool> DeleteRoleAsync(int id);
    Task AssignRolesToUserAsync(int userId, AssignRoleRequest assignRoleDto);
    Task<IEnumerable<RoleResponse>> GetUserRolesAsync(int userId);
}

