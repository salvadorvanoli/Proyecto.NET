using Shared.DTOs.BenefitTypes;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for benefit type API service.
/// </summary>
public interface IBenefitTypeApiService
{
    Task<BenefitTypeResponse?> GetBenefitTypeByIdAsync(int id);
    Task<IEnumerable<BenefitTypeResponse>> GetBenefitTypesByTenantAsync();
    Task<BenefitTypeResponse?> CreateBenefitTypeAsync(BenefitTypeRequest dto);
    Task<bool> UpdateBenefitTypeAsync(int id, BenefitTypeRequest dto);
    Task<bool> DeleteBenefitTypeAsync(int id);
}
