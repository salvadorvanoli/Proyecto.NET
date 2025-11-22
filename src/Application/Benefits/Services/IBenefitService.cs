using Application.Benefits.DTOs;

namespace Application.Benefits.Services;

/// <summary>
/// Service interface for benefit management.
/// </summary>
public interface IBenefitService
{
    /// <summary>
    /// Gets all benefits for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of benefits available for the user.</returns>
    Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId);

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    /// <returns>A list of all benefits in the tenant.</returns>
    Task<List<BenefitResponse>> GetAllBenefitsAsync();

    /// <summary>
    /// Gets a specific benefit by ID.
    /// </summary>
    /// <param name="benefitId">The ID of the benefit.</param>
    /// <returns>The benefit information.</returns>
    Task<BenefitResponse?> GetBenefitByIdAsync(int benefitId);
}
