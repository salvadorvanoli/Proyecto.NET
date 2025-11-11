using Shared.DTOs.Spaces;
using Application.Spaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing spaces.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SpacesController : ControllerBase
{
    private readonly ISpaceService _spaceService;
    private readonly ILogger<SpacesController> _logger;

    public SpacesController(ISpaceService spaceService, ILogger<SpacesController> logger)
    {
        _spaceService = spaceService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new space in the current tenant context.
    /// </summary>
    /// <param name="request">The space creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created space information.</returns>
    /// <response code="201">Space created successfully.</response>
    /// <response code="400">Invalid request or space already exists.</response>
    /// <response code="500">An error occurred while creating the space.</response>
    [HttpPost]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(SpaceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SpaceResponse>> CreateSpace(
        [FromBody] SpaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = await _spaceService.CreateSpaceAsync(request, cancellationToken);

            _logger.LogInformation("Space created successfully with ID {SpaceId} for tenant {TenantId}",
                space.Id, space.TenantId);

            return CreatedAtAction(nameof(GetSpaceById), new { id = space.Id }, space);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create space: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space");
            return StatusCode(500, new { error = "An error occurred while creating the space." });
        }
    }

    /// <summary>
    /// Gets a space by ID.
    /// </summary>
    /// <param name="id">The space ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The space information.</returns>
    /// <response code="200">Space found.</response>
    /// <response code="404">Space not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SpaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceResponse>> GetSpaceById(int id, CancellationToken cancellationToken)
    {
        var space = await _spaceService.GetSpaceByIdAsync(id, cancellationToken);

        if (space == null)
        {
            return NotFound(new { error = $"Space with ID {id} not found." });
        }

        return Ok(space);
    }

    /// <summary>
    /// Gets all spaces from the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of spaces in the current tenant.</returns>
    /// <response code="200">Spaces retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpaceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpaceResponse>>> GetSpacesByTenant(CancellationToken cancellationToken)
    {
        try
        {
            var spaces = await _spaceService.GetSpacesByTenantAsync(cancellationToken);

            _logger.LogInformation("Retrieved spaces for current tenant. Count: {Count}", spaces.Count());

            return Ok(spaces);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving spaces by tenant");
            return StatusCode(500, new { error = "An error occurred while retrieving spaces." });
        }
    }

    /// <summary>
    /// Updates an existing space.
    /// </summary>
    /// <param name="id">The space ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated space information.</returns>
    /// <response code="200">Space updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Space not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(SpaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceResponse>> UpdateSpace(
        int id,
        [FromBody] SpaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = await _spaceService.UpdateSpaceAsync(id, request, cancellationToken);

            _logger.LogInformation("Space {SpaceId} updated successfully", id);

            return Ok(space);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update space {SpaceId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating space {SpaceId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the space." });
        }
    }

    /// <summary>
    /// Deletes a space by ID.
    /// </summary>
    /// <param name="id">The space ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="204">Space deleted successfully.</response>
    /// <response code="400">Space cannot be deleted (has control points assigned).</response>
    /// <response code="404">Space not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSpace(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _spaceService.DeleteSpaceAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = $"Space with ID {id} not found." });
            }

            _logger.LogInformation("Space {SpaceId} deleted successfully", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete space {SpaceId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space {SpaceId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the space." });
        }
    }
}
