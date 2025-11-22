using Shared.DTOs.Benefits;
using Application.Benefits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    private readonly ILogger<BenefitsController> _logger;

    public BenefitsController(
        IBenefitService benefitService,
        ILogger<BenefitsController> logger)
    {
        _benefitService = benefitService;
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
    public async Task<ActionResult<IEnumerable<BenefitResponse>>> GetUserBenefits(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetUserBenefitsAsync(userId, cancellationToken);
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
    /// Gets available benefits for a user to claim (shows Quotas).
    /// </summary>
    [HttpGet("available/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AvailableBenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AvailableBenefitResponse>>> GetAvailableBenefits(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetAvailableBenefitsForUserAsync(userId, cancellationToken);
            _logger.LogInformation("Retrieved {Count} available benefits for user {UserId}", benefits.Count(), userId);
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available benefits for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving available benefits");
        }
    }

    /// <summary>
    /// Gets redeemable benefits for a user (shows Quantity from Usage).
    /// </summary>
    [HttpGet("redeemable/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<RedeemableBenefitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RedeemableBenefitResponse>>> GetRedeemableBenefits(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetRedeemableBenefitsForUserAsync(userId, cancellationToken);
            _logger.LogInformation("Retrieved {Count} redeemable benefits for user {UserId}", benefits.Count(), userId);
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving redeemable benefits for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving redeemable benefits");
        }
    }

    /// <summary>
    /// Gets benefits with consumption history for a user.
    /// </summary>
    [HttpGet("history/{userId}")]
    [ProducesResponseType(typeof(List<BenefitWithHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BenefitWithHistoryResponse>>> GetBenefitsWithHistory(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var benefits = await _benefitService.GetBenefitsWithHistoryAsync(userId, cancellationToken);
            _logger.LogInformation("Retrieved {Count} benefits with history for user {UserId}", benefits.Count, userId);
            return Ok(benefits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving benefits with history for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving benefits with history");
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
    /// Claims a benefit for a user.
    /// </summary>
    [HttpPost("claim")]
    [ProducesResponseType(typeof(ClaimBenefitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClaimBenefit([FromBody] ClaimBenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _benefitService.ClaimBenefitAsync(request, cancellationToken);
            _logger.LogInformation("Benefit {BenefitId} claimed by user {UserId}", 
                request.BenefitId, request.UserId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot claim benefit {BenefitId} for user {UserId}", 
                request.BenefitId, request.UserId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming benefit {BenefitId} for user {UserId}", 
                request.BenefitId, request.UserId);
            return StatusCode(500, "An error occurred while claiming the benefit");
        }
    }

    /// <summary>
    /// Redeems a benefit for a user.
    /// </summary>
    [HttpPost("redeem")]
    [ProducesResponseType(typeof(RedeemBenefitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedeemBenefit([FromBody] RedeemBenefitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _benefitService.RedeemBenefitAsync(request, cancellationToken);
            _logger.LogInformation("Benefit {BenefitId} redeemed by user {UserId}", 
                request.BenefitId, request.UserId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot redeem benefit {BenefitId} for user {UserId}", 
                request.BenefitId, request.UserId);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while redeeming benefit {BenefitId}", request.BenefitId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming benefit {BenefitId} for user {UserId}", 
                request.BenefitId, request.UserId);
            return StatusCode(500, "An error occurred while redeeming the benefit");
        }
    }
}
