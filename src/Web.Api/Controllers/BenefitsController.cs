using Application.Benefits.DTOs;
using Application.Benefits.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing benefits.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BenefitsController : ControllerBase
{
    private readonly BenefitService _benefitService;

    public BenefitsController(BenefitService benefitService)
    {
        _benefitService = benefitService;
    }

    /// <summary>
    /// Gets a benefit by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BenefitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenefitResponse>> GetBenefit(int id, CancellationToken cancellationToken)
    {
        var benefit = await _benefitService.GetBenefitByIdAsync(id, cancellationToken);
        
        if (benefit == null)
            return NotFound();

        return Ok(benefit);
    }

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    [HttpGet("by-tenant")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetBenefitsByTenant(CancellationToken cancellationToken)
    {
        var benefits = await _benefitService.GetBenefitsByTenantAsync(cancellationToken);
        return Ok(benefits);
    }

    /// <summary>
    /// Gets benefits filtered by benefit type.
    /// </summary>
    [HttpGet("by-type/{benefitTypeId}")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetBenefitsByType(int benefitTypeId, CancellationToken cancellationToken)
    {
        var benefits = await _benefitService.GetBenefitsByTypeAsync(benefitTypeId, cancellationToken);
        return Ok(benefits);
    }

    /// <summary>
    /// Gets active benefits (valid and with available quotas).
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetActiveBenefits(CancellationToken cancellationToken)
    {
        var benefits = await _benefitService.GetActiveBenefitsAsync(cancellationToken);
        return Ok(benefits);
    }

    /// <summary>
    /// Creates a new benefit.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BenefitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BenefitResponse>> CreateBenefit([FromBody] CreateBenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var benefit = await _benefitService.CreateBenefitAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetBenefit), new { id = benefit.Id }, benefit);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing benefit.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BenefitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenefitResponse>> UpdateBenefit(int id, [FromBody] UpdateBenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var benefit = await _benefitService.UpdateBenefitAsync(id, request, cancellationToken);
            return Ok(benefit);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a benefit.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBenefit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _benefitService.DeleteBenefitAsync(id, cancellationToken);
            
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
