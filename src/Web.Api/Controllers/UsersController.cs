using Shared.DTOs.Users;
using Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user in the current tenant context.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user information.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Invalid request or user already exists.</response>
    /// <response code="500">An error occurred while creating the user.</response>
    [HttpPost]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request, cancellationToken);

            _logger.LogInformation("User created successfully with ID {UserId} for tenant {TenantId}",
                user.Id, user.TenantId);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create user: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { error = "An error occurred while creating the user." });
        }
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user information.</returns>
    /// <response code="200">User found.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(new { error = $"User with ID {id} not found." });
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user information.</returns>
    /// <response code="200">User found.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByEmailAsync(email, cancellationToken);

        if (user == null)
        {
            return NotFound(new { error = $"User with email '{email}' not found." });
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets all users from the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of users in the current tenant.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsersByTenant(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetUsersByTenantAsync(cancellationToken);

            _logger.LogInformation("Retrieved users for current tenant. Count: {Count}", users.Count());

            return Ok(users);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to retrieve users: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by tenant");
            return StatusCode(500, new { error = "An error occurred while retrieving users." });
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user information.</returns>
    /// <response code="200">User updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request, cancellationToken);

            _logger.LogInformation("User {UserId} updated successfully", id);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update user {UserId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the user." });
        }
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _userService.DeleteUserAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = $"User with ID {id} not found." });
            }

            _logger.LogInformation("User {UserId} deleted successfully", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the user." });
        }
    }

    /// <summary>
    /// Assigns a credential to a user (creates a new credential if needed).
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    [HttpPost("{id}/assign-credential")]
    [Authorize(Roles = "AdministradorBackoffice")]
    public async Task<IActionResult> AssignCredential(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.AssignCredentialToUserAsync(id, cancellationToken);
            _logger.LogInformation("Credential assigned to user {UserId}", id);
            return Ok(new { message = "Credential assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning credential to user {UserId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
