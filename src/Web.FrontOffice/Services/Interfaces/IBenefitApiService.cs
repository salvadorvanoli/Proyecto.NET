using Application.Benefits.DTOs;

namespace Web.FrontOffice.Services.Interfaces;

/// <summary>
/// Service interface for benefit API communication.
/// </summary>
public interface IBenefitApiService
{
    /// <summary>
    /// Gets all benefits for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of benefits.</returns>
    Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId);

    /// <summary>
    /// Gets paginated benefits for a specific user with optional search.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="searchTerm">Optional search term to filter benefits.</param>
    /// <returns>A tuple containing the list of benefits and the total count.</returns>
    Task<(List<BenefitResponse> Benefits, int TotalCount)> GetUserBenefitsPagedAsync(
        int userId, 
        int skip = 0, 
        int take = 10, 
        string? searchTerm = null);

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    /// <returns>A list of all benefits.</returns>
    Task<List<BenefitResponse>> GetAllBenefitsAsync();

    /// <summary>
    /// Gets a specific benefit by ID.
    /// </summary>
    /// <param name="benefitId">The ID of the benefit.</param>
    /// <returns>The benefit information.</returns>
    Task<BenefitResponse?> GetBenefitByIdAsync(int benefitId);
}
