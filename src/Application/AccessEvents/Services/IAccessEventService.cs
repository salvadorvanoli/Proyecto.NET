using Application.AccessEvents.DTOs;

namespace Application.AccessEvents.Services;

/// <summary>
/// Service interface for access event management.
/// </summary>
public interface IAccessEventService
{
    /// <summary>
    /// Gets all access events for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of access events for the user.</returns>
    Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId);

    /// <summary>
    /// Gets all access events for the current tenant.
    /// </summary>
    /// <returns>A list of all access events in the tenant.</returns>
    Task<List<AccessEventResponse>> GetAllAccessEventsAsync();

    /// <summary>
    /// Gets a specific access event by ID.
    /// </summary>
    /// <param name="eventId">The ID of the access event.</param>
    /// <returns>The access event information.</returns>
    Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId);
}
