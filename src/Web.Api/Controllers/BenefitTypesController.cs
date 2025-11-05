using Application.BenefitTypes.DTOs;
using Application.BenefitTypes.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing benefit types.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BenefitTypesController : ControllerBase
{
    private readonly IBenefitTypeService _benefitTypeService;
    private readonly ILogger<BenefitTypesController> _logger;

    public BenefitTypesController(IBenefitTypeService benefitTypeService, ILogger<BenefitTypesController> logger)
    {
        _benefitTypeService = benefitTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all benefit types for the current tenant.
    /// </summary>
    /// <returns>List of benefit types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenefitTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitTypeResponse>>> GetBenefitTypes()
    {
        try
        {
            var benefitTypes = await _benefitTypeService.GetBenefitTypesByTenantAsync();
            _logger.LogInformation("Retrieved {Count} benefit types", benefitTypes.Count());
            return Ok(benefitTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving benefit types");
            return StatusCode(500, "An error occurred while retrieving benefit types");
        }
    }

    /// <summary>
    /// Gets a specific benefit type by ID.
    /// </summary>
    /// <param name="id">The benefit type ID.</param>
    /// <returns>The benefit type details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BenefitTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenefitTypeResponse>> GetBenefitTypeById(int id)
    {
        var benefitType = await _benefitTypeService.GetBenefitTypeByIdAsync(id);
        
        if (benefitType == null)
        {
            return NotFound(new { message = $"Benefit type with ID {id} not found." });
        }
        
        return Ok(benefitType);
    }

    /// <summary>
    /// Creates a new benefit type.
    /// </summary>
    /// <param name="request">The benefit type creation request.</param>
    /// <returns>The created benefit type.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BenefitTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BenefitTypeResponse>> CreateBenefitType([FromBody] CreateBenefitTypeRequest request)
    {
        try
        {
            var benefitType = await _benefitTypeService.CreateBenefitTypeAsync(request);
            _logger.LogInformation("Created benefit type with ID {Id}", benefitType.Id);
            return CreatedAtAction(nameof(GetBenefitTypeById), new { id = benefitType.Id }, benefitType);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating benefit type");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating benefit type");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while creating the benefit type.", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing benefit type.
    /// </summary>
    /// <param name="id">The benefit type ID.</param>
    /// <param name="request">The benefit type update request.</param>
    /// <returns>The updated benefit type.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BenefitTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BenefitTypeResponse>> UpdateBenefitType(int id, [FromBody] UpdateBenefitTypeRequest request)
    {
        try
        {
            var benefitType = await _benefitTypeService.UpdateBenefitTypeAsync(id, request);
            _logger.LogInformation("Updated benefit type with ID {Id}", id);
            return Ok(benefitType);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Benefit type with ID {Id} not found", id);
                return NotFound(new { message = ex.Message });
            }
            
            _logger.LogWarning(ex, "Invalid operation while updating benefit type with ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating benefit type with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while updating the benefit type.", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a benefit type.
    /// </summary>
    /// <param name="id">The benefit type ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBenefitType(int id)
    {
        try
        {
            var result = await _benefitTypeService.DeleteBenefitTypeAsync(id);
            
            if (!result)
            {
                _logger.LogWarning("Benefit type with ID {Id} not found", id);
                return NotFound(new { message = $"Benefit type with ID {id} not found." });
            }
            
            _logger.LogInformation("Deleted benefit type with ID {Id}", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete benefit type with ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting benefit type with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the benefit type.", details = ex.Message });
        }
    }
}
