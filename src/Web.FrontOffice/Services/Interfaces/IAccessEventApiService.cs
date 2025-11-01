using Application.AccessEvents.DTOs;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Service interface for access event API communication.
/// </summary>
public interface IAccessEventApiService
{
    /// <summary>
    /// Gets all access events for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of access events.</returns>
    Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId);

    /// <summary>
    /// Gets all access events for the current tenant.
    /// </summary>
    /// <returns>A list of all access events.</returns>
    Task<List<AccessEventResponse>> GetAllAccessEventsAsync();

    /// <summary>
    /// Gets a specific access event by ID.
    /// </summary>
    /// <param name="eventId">The ID of the access event.</param>
    /// <returns>The access event information.</returns>
    Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId);
}
