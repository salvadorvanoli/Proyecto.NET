using Application.AccessEvents.DTOs;
using Application.AccessEvents.Services;
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
    private readonly ILogger<AccessEventsController> _logger;

    public AccessEventsController(IAccessEventService accessEventService, ILogger<AccessEventsController> logger)
    {
        _accessEventService = accessEventService;
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
}
