using Shared.DTOs.Benefits;
using Application.Benefits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BenefitQueryService = Application.Benefits.Services.IBenefitService;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing benefits.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BenefitsController : ControllerBase
{
    private readonly IBenefitService _benefitService;
    private readonly BenefitQueryService _benefitQueryService;
    private readonly ILogger<BenefitsController> _logger;

    public BenefitsController(
        IBenefitService benefitService,
        BenefitQueryService benefitQueryService,
        ILogger<BenefitsController> logger)
    {
        _benefitService = benefitService;
        _benefitQueryService = benefitQueryService;
        _logger = logger;
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
        {
            return NotFound();
        }

        return Ok(benefit);
    }

    /// <summary>
    /// Gets all benefits for a specific user.
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetUserBenefits(int userId)
    {
        try
        {
            var benefits = await _benefitQueryService.GetUserBenefitsAsync(userId);
            _logger.LogInformation("Retrieved {Count} benefits for user {UserId}", benefits.Count, userId);
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving benefits for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user benefits");
        }
    }

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    [HttpGet("by-tenant")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetBenefitsByTenant(CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetBenefitsByTenantAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} benefits for tenant", benefits.Count());
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving benefits by tenant");
            return StatusCode(500, "An error occurred while retrieving benefits");
        }
    }

    /// <summary>
    /// Gets benefits filtered by benefit type.
    /// </summary>
    [HttpGet("by-type/{benefitTypeId}")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetBenefitsByType(int benefitTypeId, CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetBenefitsByTypeAsync(benefitTypeId, cancellationToken);
            _logger.LogInformation("Retrieved {Count} benefits for benefit type {BenefitTypeId}", benefits.Count(), benefitTypeId);
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving benefits for benefit type {BenefitTypeId}", benefitTypeId);
            return StatusCode(500, "An error occurred while retrieving benefits");
        }
    }

    /// <summary>
    /// Gets active benefits (valid and with available quotas).
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<BenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetActiveBenefits(CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetActiveBenefitsAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} active benefits", benefits.Count());
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active benefits");
            return StatusCode(500, "An error occurred while retrieving active benefits");
        }
    }

    /// <summary>
    /// Creates a new benefit.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(BenefitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BenefitResponse>> CreateBenefit([FromBody] BenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var benefit = await _benefitService.CreateBenefitAsync(request, cancellationToken);
            _logger.LogInformation("Created benefit with ID {Id}", benefit.Id);
            return CreatedAtAction(nameof(GetBenefit), new { id = benefit.Id }, benefit);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating benefit");
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while creating benefit");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating benefit");
            return StatusCode(500, "An error occurred while creating the benefit");
        }
    }

    /// <summary>
    /// Updates an existing benefit.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(BenefitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenefitResponse>> UpdateBenefit(int id, [FromBody] BenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var benefit = await _benefitService.UpdateBenefitAsync(id, request, cancellationToken);
            _logger.LogInformation("Updated benefit with ID {Id}", id);
            return Ok(benefit);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Benefit with ID {Id} not found", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating benefit with ID {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating benefit with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the benefit");
        }
    }

    /// <summary>
    /// Deletes a benefit.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBenefit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _benefitService.DeleteBenefitAsync(id, cancellationToken);
            
            if (!result)
            {
                _logger.LogWarning("Benefit with ID {Id} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Deleted benefit with ID {Id}", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete benefit with ID {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting benefit with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the benefit");
        }
    }

    /// <summary>
    /// Consumes a benefit for the authenticated user.
    /// </summary>
    [HttpPost("consume")]
    [ProducesResponseType(typeof(ConsumeBenefitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsumeBenefitResponse>> ConsumeBenefit([FromBody] ConsumeBenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the authenticated user's ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Failed to extract user ID from token");
                return Unauthorized(new { error = "Usuario no autenticado." });
            }

            var result = await _benefitService.ConsumeBenefitAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("User {UserId} consumed benefit {BenefitId}, quantity: {Quantity}", 
                userId, request.BenefitId, request.Quantity);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while consuming benefit");
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while consuming benefit");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming benefit");
            return StatusCode(500, new { error = "Ocurrió un error al consumir el beneficio." });
        }
    }
}
