using Shared.DTOs.Roles;
using Application.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdministradorBackoffice")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new role in the current tenant context.
    /// </summary>
    /// <param name="request">The role creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created role information.</returns>
    /// <response code="201">Role created successfully.</response>
    /// <response code="400">Invalid request or role already exists.</response>
    /// <response code="500">An error occurred while creating the role.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleResponse>> CreateRole(
        [FromBody] RoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(request, cancellationToken);

            _logger.LogInformation("Role created successfully with ID {RoleId} for tenant {TenantId}",
                role.Id, role.TenantId);

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create role: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { error = "An error occurred while creating the role." });
        }
    }

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role information.</returns>
    /// <response code="200">Role found.</response>
    /// <response code="404">Role not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetRoleById(int id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);

        if (role == null)
        {
            return NotFound(new { error = $"Role with ID {id} not found." });
        }

        return Ok(role);
    }

    /// <summary>
    /// Gets all roles from the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of roles in the current tenant.</returns>
    /// <response code="200">Roles retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRolesByTenant(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetRolesByTenantAsync(cancellationToken);

            _logger.LogInformation("Retrieved roles for current tenant. Count: {Count}", roles.Count());

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles by tenant");
            return StatusCode(500, new { error = "An error occurred while retrieving roles." });
        }
    }

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role information.</returns>
    /// <response code="200">Role updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Role not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> UpdateRole(
        int id,
        [FromBody] RoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.UpdateRoleAsync(id, request, cancellationToken);

            _logger.LogInformation("Role {RoleId} updated successfully", id);

            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update role {RoleId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the role." });
        }
    }

    /// <summary>
    /// Deletes a role by ID.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="204">Role deleted successfully.</response>
    /// <response code="400">Role cannot be deleted (has users assigned).</response>
    /// <response code="404">Role not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _roleService.DeleteRoleAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = $"Role with ID {id} not found." });
            }

            _logger.LogInformation("Role {RoleId} deleted successfully", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete role {RoleId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the role." });
        }
    }

    /// <summary>
    /// Assigns roles to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The role assignment request containing role IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Roles assigned successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("assign/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRolesToUser(
        int userId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.AssignRolesToUserAsync(userId, request, cancellationToken);

            _logger.LogInformation("Roles assigned to user {UserId} successfully", userId);

            return Ok(new { message = "Roles assigned successfully." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to assign roles to user {UserId}: {Message}", userId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while assigning roles." });
        }
    }

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of roles assigned to the user.</returns>
    /// <response code="200">Roles retrieved successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetUserRoles(
        int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetUserRolesAsync(userId, cancellationToken);

            _logger.LogInformation("Retrieved roles for user {UserId}. Count: {Count}", userId, roles.Count());

            return Ok(roles);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to get roles for user {UserId}: {Message}", userId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving user roles." });
        }
    }
}

