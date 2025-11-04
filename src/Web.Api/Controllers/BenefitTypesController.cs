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

    public BenefitTypesController(IBenefitTypeService benefitTypeService)
    {
        _benefitTypeService = benefitTypeService;
    }

    /// <summary>
    /// Gets all benefit types for the current tenant.
    /// </summary>
    /// <returns>List of benefit types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenefitTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitTypeResponse>>> GetBenefitTypes()
    {
        var benefitTypes = await _benefitTypeService.GetBenefitTypesByTenantAsync();
        return Ok(benefitTypes);
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
            return NotFound(new { message = $"Benefit type with ID {id} not found." });
        
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
            return CreatedAtAction(nameof(GetBenefitTypeById), new { id = benefitType.Id }, benefitType);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
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
            return Ok(benefitType);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });
            
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
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
                return NotFound(new { message = $"Benefit type with ID {id} not found." });
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the benefit type.", details = ex.Message });
        }
    }
}
