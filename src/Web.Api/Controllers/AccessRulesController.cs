using Application.AccessRules.DTOs;
using Application.AccessRules.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing access rules.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccessRulesController : ControllerBase
{
    private readonly IAccessRuleService _accessRuleService;
    private readonly ILogger<AccessRulesController> _logger;

    public AccessRulesController(IAccessRuleService accessRuleService, ILogger<AccessRulesController> logger)
    {
        _accessRuleService = accessRuleService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all access rules for the current tenant.
    /// </summary>
    /// <returns>List of access rules.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccessRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccessRuleResponse>>> GetAccessRules()
    {
        try
        {
            var accessRules = await _accessRuleService.GetAccessRulesByTenantAsync();
            _logger.LogInformation("Retrieved {Count} access rules", accessRules.Count());
            return Ok(accessRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access rules");
            return StatusCode(500, "An error occurred while retrieving access rules");
        }
    }

    /// <summary>
    /// Gets a specific access rule by ID.
    /// </summary>
    /// <param name="id">The access rule ID.</param>
    /// <returns>The access rule details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccessRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessRuleResponse>> GetAccessRuleById(int id)
    {
        var accessRule = await _accessRuleService.GetAccessRuleByIdAsync(id);
        
        if (accessRule == null)
        {
            return NotFound(new { message = $"Access rule with ID {id} not found." });
        }
        
        return Ok(accessRule);
    }

    /// <summary>
    /// Gets all access rules for a specific control point.
    /// </summary>
    /// <param name="controlPointId">The control point ID.</param>
    /// <returns>List of access rules for the control point.</returns>
    [HttpGet("controlpoint/{controlPointId}")]
    [ProducesResponseType(typeof(IEnumerable<AccessRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccessRuleResponse>>> GetAccessRulesByControlPoint(int controlPointId)
    {
        try
        {
            var accessRules = await _accessRuleService.GetAccessRulesByControlPointAsync(controlPointId);
            _logger.LogInformation("Retrieved {Count} access rules for control point {ControlPointId}", accessRules.Count(), controlPointId);
            return Ok(accessRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access rules for control point {ControlPointId}", controlPointId);
            return StatusCode(500, "An error occurred while retrieving access rules");
        }
    }

    /// <summary>
    /// Gets all access rules for a specific role.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <returns>List of access rules for the role.</returns>
    [HttpGet("role/{roleId}")]
    [ProducesResponseType(typeof(IEnumerable<AccessRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccessRuleResponse>>> GetAccessRulesByRole(int roleId)
    {
        try
        {
            var accessRules = await _accessRuleService.GetAccessRulesByRoleAsync(roleId);
            _logger.LogInformation("Retrieved {Count} access rules for role {RoleId}", accessRules.Count(), roleId);
            return Ok(accessRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access rules for role {RoleId}", roleId);
            return StatusCode(500, "An error occurred while retrieving access rules");
        }
    }

    /// <summary>
    /// Creates a new access rule.
    /// </summary>
    /// <param name="request">The access rule creation request.</param>
    /// <returns>The created access rule.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccessRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessRuleResponse>> CreateAccessRule([FromBody] CreateAccessRuleRequest request)
    {
        try
        {
            var accessRule = await _accessRuleService.CreateAccessRuleAsync(request);
            _logger.LogInformation("Created access rule with ID {Id}", accessRule.Id);
            return CreatedAtAction(nameof(GetAccessRuleById), new { id = accessRule.Id }, accessRule);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating access rule");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access rule");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while creating the access rule.", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing access rule.
    /// </summary>
    /// <param name="id">The access rule ID.</param>
    /// <param name="request">The access rule update request.</param>
    /// <returns>The updated access rule.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AccessRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessRuleResponse>> UpdateAccessRule(int id, [FromBody] UpdateAccessRuleRequest request)
    {
        try
        {
            var accessRule = await _accessRuleService.UpdateAccessRuleAsync(id, request);
            _logger.LogInformation("Updated access rule with ID {Id}", id);
            return Ok(accessRule);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Access rule with ID {Id} not found", id);
                return NotFound(new { message = ex.Message });
            }
            
            _logger.LogWarning(ex, "Invalid operation while updating access rule with ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating access rule with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while updating the access rule.", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an access rule.
    /// </summary>
    /// <param name="id">The access rule ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAccessRule(int id)
    {
        try
        {
            var result = await _accessRuleService.DeleteAccessRuleAsync(id);
            
            if (!result)
            {
                _logger.LogWarning("Access rule with ID {Id} not found", id);
                return NotFound(new { message = $"Access rule with ID {id} not found." });
            }
            
            _logger.LogInformation("Deleted access rule with ID {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting access rule with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the access rule.", details = ex.Message });
        }
    }
}
