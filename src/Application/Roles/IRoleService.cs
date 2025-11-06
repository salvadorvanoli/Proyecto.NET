using Shared.DTOs.Roles;

namespace Application.Roles;

/// <summary>
/// Service interface for role operations.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates a new role in the current tenant context.
    /// </summary>
    Task<RoleResponse> CreateRoleAsync(RoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    Task<RoleResponse?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles from the current tenant.
    /// </summary>
    Task<IEnumerable<RoleResponse>> GetRolesByTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    Task<RoleResponse> UpdateRoleAsync(int id, RoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role by ID.
    /// </summary>
    Task<bool> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns roles to a user.
    /// </summary>
    Task AssignRolesToUserAsync(int userId, AssignRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    Task<IEnumerable<RoleResponse>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
}
