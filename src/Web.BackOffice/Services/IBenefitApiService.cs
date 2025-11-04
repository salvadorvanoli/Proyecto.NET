using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for benefit API service.
/// </summary>
public interface IBenefitApiService
{
    Task<BenefitDto?> GetBenefitByIdAsync(int id);
    Task<IEnumerable<BenefitDto>> GetBenefitsByTenantAsync();
    Task<IEnumerable<BenefitDto>> GetBenefitsByTypeAsync(int benefitTypeId);
    Task<IEnumerable<BenefitDto>> GetActiveBenefitsAsync();
    Task<BenefitDto> CreateBenefitAsync(CreateBenefitDto benefit);
    Task<bool> UpdateBenefitAsync(int id, UpdateBenefitDto benefit);
    Task<bool> DeleteBenefitAsync(int id);
}
