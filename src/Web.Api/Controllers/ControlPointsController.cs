using Application.ControlPoints.DTOs;
using Application.ControlPoints.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing control points.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ControlPointsController : ControllerBase
{
    private readonly IControlPointService _controlPointService;
    private readonly ILogger<ControlPointsController> _logger;

    public ControlPointsController(IControlPointService controlPointService, ILogger<ControlPointsController> logger)
    {
        _controlPointService = controlPointService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new control point in the current tenant context.
    /// </summary>
    /// <param name="request">The control point creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created control point information.</returns>
    /// <response code="201">Control point created successfully.</response>
    /// <response code="400">Invalid request or control point already exists.</response>
    /// <response code="500">An error occurred while creating the control point.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ControlPointResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ControlPointResponse>> CreateControlPoint(
        [FromBody] CreateControlPointRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var controlPoint = await _controlPointService.CreateControlPointAsync(request, cancellationToken);

            _logger.LogInformation("Control point created successfully with ID {ControlPointId} for tenant {TenantId}",
                controlPoint.Id, controlPoint.TenantId);

            return CreatedAtAction(nameof(GetControlPointById), new { id = controlPoint.Id }, controlPoint);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create control point: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating control point");
            return StatusCode(500, new { error = "An error occurred while creating the control point." });
        }
    }

    /// <summary>
    /// Gets a control point by ID.
    /// </summary>
    /// <param name="id">The control point ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The control point information.</returns>
    /// <response code="200">Control point found.</response>
    /// <response code="404">Control point not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ControlPointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ControlPointResponse>> GetControlPointById(int id, CancellationToken cancellationToken)
    {
        var controlPoint = await _controlPointService.GetControlPointByIdAsync(id, cancellationToken);

        if (controlPoint == null)
        {
            return NotFound(new { error = $"Control point with ID {id} not found." });
        }

        return Ok(controlPoint);
    }

    /// <summary>
    /// Gets all control points from the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of control points in the current tenant.</returns>
    /// <response code="200">Control points retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ControlPointResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ControlPointResponse>>> GetControlPointsByTenant(CancellationToken cancellationToken)
    {
        try
        {
            var controlPoints = await _controlPointService.GetControlPointsByTenantAsync(cancellationToken);

            _logger.LogInformation("Retrieved control points for current tenant. Count: {Count}", controlPoints.Count());

            return Ok(controlPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving control points by tenant");
            return StatusCode(500, new { error = "An error occurred while retrieving control points." });
        }
    }

    /// <summary>
    /// Updates an existing control point.
    /// </summary>
    /// <param name="id">The control point ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated control point information.</returns>
    /// <response code="200">Control point updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Control point not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ControlPointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ControlPointResponse>> UpdateControlPoint(
        int id,
        [FromBody] UpdateControlPointRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var controlPoint = await _controlPointService.UpdateControlPointAsync(id, request, cancellationToken);

            _logger.LogInformation("Control point {ControlPointId} updated successfully", id);

            return Ok(controlPoint);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update control point {ControlPointId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating control point {ControlPointId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the control point." });
        }
    }

    /// <summary>
    /// Deletes a control point by ID.
    /// </summary>
    /// <param name="id">The control point ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="204">Control point deleted successfully.</response>
    /// <response code="400">Control point cannot be deleted (has access rules or events).</response>
    /// <response code="404">Control point not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteControlPoint(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _controlPointService.DeleteControlPointAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = $"Control point with ID {id} not found." });
            }

            _logger.LogInformation("Control point {ControlPointId} deleted successfully", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete control point {ControlPointId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting control point {ControlPointId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the control point." });
        }
    }
}
