using Application.Users.DTOs;
using Application.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// Gets all users from all tenants (admin operation).
    /// Warning: This endpoint returns users from ALL tenants.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all users.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);

            _logger.LogInformation("Retrieved all users from all tenants. Count: {Count}", users.Count());

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, new { error = "An error occurred while retrieving users." });
        }
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
}
