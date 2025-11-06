using Shared.DTOs.Benefits;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for benefit API service.
/// </summary>
public interface IBenefitApiService
{
    Task<BenefitResponse?> GetBenefitByIdAsync(int id);
    Task<IEnumerable<BenefitResponse>> GetBenefitsByTenantAsync();
    Task<IEnumerable<BenefitResponse>> GetBenefitsByTypeAsync(int benefitTypeId);
    Task<IEnumerable<BenefitResponse>> GetActiveBenefitsAsync();
    Task<BenefitResponse> CreateBenefitAsync(BenefitRequest benefit);
    Task<bool> UpdateBenefitAsync(int id, BenefitRequest benefit);
    Task<bool> DeleteBenefitAsync(int id);
}
