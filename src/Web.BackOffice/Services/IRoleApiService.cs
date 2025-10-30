using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Service for managing roles through the API.
/// </summary>
public interface IRoleApiService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto updateRoleDto);
    Task<bool> DeleteRoleAsync(int id);
    Task AssignRolesToUserAsync(int userId, AssignRoleDto assignRoleDto);
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(int userId);
}

