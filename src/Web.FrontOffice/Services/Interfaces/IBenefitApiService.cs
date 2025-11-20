using Shared.DTOs.Benefits;
using BenefitDto = Application.Benefits.DTOs.BenefitResponse;

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
    Task<List<BenefitDto>> GetUserBenefitsAsync(int userId);

    /// <summary>
    /// Gets paginated benefits for a specific user with optional search.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="searchTerm">Optional search term to filter benefits.</param>
    /// <returns>A tuple containing the list of benefits and the total count.</returns>
    Task<(List<BenefitDto> Benefits, int TotalCount)> GetUserBenefitsPagedAsync(
        int userId, 
        int skip = 0, 
        int take = 10, 
        string? searchTerm = null);

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    /// <returns>A list of all benefits.</returns>
    Task<List<BenefitDto>> GetAllBenefitsAsync();

    /// <summary>
    /// Gets a specific benefit by ID.
    /// </summary>
    /// <param name="benefitId">The ID of the benefit.</param>
    /// <returns>The benefit information.</returns>
    Task<BenefitDto?> GetBenefitByIdAsync(int benefitId);

    /// <summary>
    /// Redeems a benefit for a user.
    /// </summary>
    /// <param name="request">The redeem benefit request.</param>
    /// <returns>The redeem benefit response.</returns>
    Task<RedeemBenefitResponse> RedeemBenefitAsync(RedeemBenefitRequest request);
}
