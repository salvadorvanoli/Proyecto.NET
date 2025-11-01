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

    public AccessEventsController(IAccessEventService accessEventService)
    {
        _accessEventService = accessEventService;
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
}
