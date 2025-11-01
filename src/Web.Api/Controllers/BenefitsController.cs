using Application.Benefits.DTOs;
using Application.Benefits.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing benefits.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BenefitsController : ControllerBase
{
    private readonly IBenefitService _benefitService;

    public BenefitsController(IBenefitService benefitService)
    {
        _benefitService = benefitService;
    }

    /// <summary>
    /// Gets all benefits for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of benefits.</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<BenefitResponse>>> GetUserBenefits(int userId)
    {
        try
        {
            var benefits = await _benefitService.GetUserBenefitsAsync(userId);
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving user benefits", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    /// <returns>A list of all benefits.</returns>
    [HttpGet]
    public async Task<ActionResult<List<BenefitResponse>>> GetAllBenefits()
    {
        try
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving benefits", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific benefit by ID.
    /// </summary>
    /// <param name="id">The ID of the benefit.</param>
    /// <returns>The benefit information.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BenefitResponse>> GetBenefit(int id)
    {
        try
        {
            var benefit = await _benefitService.GetBenefitByIdAsync(id);
            
            if (benefit == null)
                return NotFound(new { message = "Benefit not found" });
            
            return Ok(benefit);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving benefit", error = ex.Message });
        }
    }
}
