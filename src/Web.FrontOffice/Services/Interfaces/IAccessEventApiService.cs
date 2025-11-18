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
    /// Gets paginated access events for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <returns>A tuple containing the list of access events and the total count.</returns>
    Task<(List<AccessEventResponse> Events, int TotalCount)> GetUserAccessEventsPagedAsync(
        int userId, 
        int skip = 0, 
        int take = 20);

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

    /// <summary>
    /// Creates a new access event.
    /// </summary>
    /// <param name="request">The access event data.</param>
    /// <returns>The created access event.</returns>
    Task<AccessEventResponse> CreateAccessEventAsync(CreateAccessEventRequest request);
}
