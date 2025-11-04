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

    public AccessRulesController(IAccessRuleService accessRuleService)
    {
        _accessRuleService = accessRuleService;
    }

    /// <summary>
    /// Gets all access rules for the current tenant.
    /// </summary>
    /// <returns>List of access rules.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccessRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccessRuleResponse>>> GetAccessRules()
    {
        var accessRules = await _accessRuleService.GetAccessRulesByTenantAsync();
        return Ok(accessRules);
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
            return NotFound(new { message = $"Access rule with ID {id} not found." });
        
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
        var accessRules = await _accessRuleService.GetAccessRulesByControlPointAsync(controlPointId);
        return Ok(accessRules);
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
        var accessRules = await _accessRuleService.GetAccessRulesByRoleAsync(roleId);
        return Ok(accessRules);
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
            return CreatedAtAction(nameof(GetAccessRuleById), new { id = accessRule.Id }, accessRule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
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
            return Ok(accessRule);
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
                return NotFound(new { message = $"Access rule with ID {id} not found." });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the access rule.", details = ex.Message });
        }
    }
}
