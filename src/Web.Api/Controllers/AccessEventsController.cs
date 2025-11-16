using Application.AccessEvents.DTOs;
using Application.AccessEvents.Services;
using Application.AccessEvents;
using Application.AccessRules;
using Shared.DTOs.AccessEvents;
using Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing access events.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccessEventsController : ControllerBase
{
    private readonly IAccessEventService _accessEventService;
    private readonly IAccessValidationService _accessValidationService;
    private readonly IAccessRuleService _accessRuleService;
    private readonly ILogger<AccessEventsController> _logger;

    public AccessEventsController(
        IAccessEventService accessEventService,
        IAccessValidationService accessValidationService,
        IAccessRuleService accessRuleService,
        ILogger<AccessEventsController> logger)
    {
        _accessEventService = accessEventService;
        _accessValidationService = accessValidationService;
        _accessRuleService = accessRuleService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all access events for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of access events.</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<AccessEventResponse>>> GetUserAccessEvents(int userId)
    {
        try
        {
            var events = await _accessEventService.GetUserAccessEventsAsync(userId);
            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving user access events", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all access events for the current tenant.
    /// </summary>
    /// <returns>A list of all access events.</returns>
    [HttpGet]
    public async Task<ActionResult<List<AccessEventResponse>>> GetAllAccessEvents()
    {
        try
        {
            var events = await _accessEventService.GetAllAccessEventsAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving access events", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific access event by ID.
    /// </summary>
    /// <param name="id">The ID of the access event.</param>
    /// <returns>The access event information.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AccessEventResponse>> GetAccessEvent(int id)
    {
        try
        {
            var accessEvent = await _accessEventService.GetAccessEventByIdAsync(id);
            
            if (accessEvent == null)
                return NotFound(new { message = "Access event not found" });
            
            return Ok(accessEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving access event", error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new access event.
    /// </summary>
    /// <param name="request">The access event data.</param>
    /// <returns>The created access event.</returns>
    [HttpPost]
    public async Task<ActionResult<AccessEventResponse>> CreateAccessEvent([FromBody] CreateAccessEventRequest request)
    {
        try
        {
            _logger.LogInformation("Creating access event for user {UserId} at control point {ControlPointId}", 
                request.UserId, request.ControlPointId);

            var accessEvent = await _accessEventService.CreateAccessEventAsync(request);
            
            _logger.LogInformation("Access event {EventId} created successfully. Result: {Result}", 
                accessEvent.Id, accessEvent.Result);

            return CreatedAtAction(
                nameof(GetAccessEvent),
                new { id = accessEvent.Id },
                accessEvent);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request when creating access event");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access event");
            return StatusCode(500, new { message = "Error creating access event", error = ex.Message });
        }
    }

    /// <summary>
    /// Validates if a user has access to a specific control point.
    /// </summary>
    /// <param name="userId">The user ID attempting access.</param>
    /// <param name="controlPointId">The control point ID to access.</param>
    /// <returns>Validation result with access decision and reason.</returns>
    [HttpPost("validate")]
    public async Task<ActionResult<AccessValidationResult>> ValidateAccess(
        [FromBody] ValidateAccessRequest request)
    {
        try
        {
            _logger.LogInformation("Validating access for user {UserId} to control point {ControlPointId}", 
                request.UserId, request.ControlPointId);

            var validationResult = await _accessValidationService.ValidateAccessAsync(request.UserId, request.ControlPointId);
            
            _logger.LogInformation("Access validation completed. User {UserId}, ControlPoint {ControlPointId}, Result: {Result}, Reason: {Reason}", 
                request.UserId, request.ControlPointId, validationResult.Result, validationResult.Reason);

            return Ok(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access for user {UserId} to control point {ControlPointId}", 
                request.UserId, request.ControlPointId);
            return StatusCode(500, new { message = "Error validating access", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Gets all active access rules for offline caching.
    /// </summary>
    /// <returns>List of access rules with user, control point, and time restrictions.</returns>
    [HttpGet("rules")]
    public async Task<ActionResult<List<AccessRuleDto>>> GetAccessRules()
    {
        try
        {
            _logger.LogInformation("Getting all active access rules for offline sync");
            
            var rules = await _accessRuleService.GetAllActiveRulesAsync();
            
            _logger.LogInformation("Retrieved {Count} active access rules", rules.Count);
            
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access rules");
            return StatusCode(500, new { message = "Error retrieving access rules", error = ex.Message });
        }
    }
}

