using Application.SpaceTypes.DTOs;
using Application.SpaceTypes.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing space types.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SpaceTypesController : ControllerBase
{
    private readonly ISpaceTypeService _spaceTypeService;
    private readonly ILogger<SpaceTypesController> _logger;

    public SpaceTypesController(ISpaceTypeService spaceTypeService, ILogger<SpaceTypesController> logger)
    {
        _spaceTypeService = spaceTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new space type in the current tenant context.
    /// </summary>
    /// <param name="request">The space type creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created space type information.</returns>
    /// <response code="201">Space type created successfully.</response>
    /// <response code="400">Invalid request or space type already exists.</response>
    /// <response code="500">An error occurred while creating the space type.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SpaceTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SpaceTypeResponse>> CreateSpaceType(
        [FromBody] CreateSpaceTypeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var spaceType = await _spaceTypeService.CreateSpaceTypeAsync(request, cancellationToken);

            _logger.LogInformation("Space type created successfully with ID {SpaceTypeId} for tenant {TenantId}",
                spaceType.Id, spaceType.TenantId);

            return CreatedAtAction(nameof(GetSpaceTypeById), new { id = spaceType.Id }, spaceType);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create space type: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space type");
            return StatusCode(500, new { error = "An error occurred while creating the space type." });
        }
    }

    /// <summary>
    /// Gets a space type by ID.
    /// </summary>
    /// <param name="id">The space type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The space type information.</returns>
    /// <response code="200">Space type found.</response>
    /// <response code="404">Space type not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SpaceTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceTypeResponse>> GetSpaceTypeById(int id, CancellationToken cancellationToken)
    {
        var spaceType = await _spaceTypeService.GetSpaceTypeByIdAsync(id, cancellationToken);

        if (spaceType == null)
        {
            return NotFound(new { error = $"Space type with ID {id} not found." });
        }

        return Ok(spaceType);
    }

    /// <summary>
    /// Gets all space types from the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of space types in the current tenant.</returns>
    /// <response code="200">Space types retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpaceTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpaceTypeResponse>>> GetSpaceTypesByTenant(CancellationToken cancellationToken)
    {
        try
        {
            var spaceTypes = await _spaceTypeService.GetSpaceTypesByTenantAsync(cancellationToken);

            _logger.LogInformation("Retrieved space types for current tenant. Count: {Count}", spaceTypes.Count());

            return Ok(spaceTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving space types by tenant");
            return StatusCode(500, new { error = "An error occurred while retrieving space types." });
        }
    }

    /// <summary>
    /// Updates an existing space type.
    /// </summary>
    /// <param name="id">The space type ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated space type information.</returns>
    /// <response code="200">Space type updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Space type not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SpaceTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpaceTypeResponse>> UpdateSpaceType(
        int id,
        [FromBody] UpdateSpaceTypeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var spaceType = await _spaceTypeService.UpdateSpaceTypeAsync(id, request, cancellationToken);

            _logger.LogInformation("Space type {SpaceTypeId} updated successfully", id);

            return Ok(spaceType);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update space type {SpaceTypeId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating space type {SpaceTypeId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the space type." });
        }
    }

    /// <summary>
    /// Deletes a space type by ID.
    /// </summary>
    /// <param name="id">The space type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="204">Space type deleted successfully.</response>
    /// <response code="400">Space type cannot be deleted (has spaces assigned).</response>
    /// <response code="404">Space type not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSpaceType(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _spaceTypeService.DeleteSpaceTypeAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = $"Space type with ID {id} not found." });
            }

            _logger.LogInformation("Space type {SpaceTypeId} deleted successfully", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete space type {SpaceTypeId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space type {SpaceTypeId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the space type." });
        }
    }
}
