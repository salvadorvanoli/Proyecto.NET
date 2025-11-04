using Web.BackOffice.Models;

namespace Web.BackOffice.Services;

/// <summary>
/// Interface for benefit type API service.
/// </summary>
public interface IBenefitTypeApiService
{
    Task<BenefitTypeDto?> GetBenefitTypeByIdAsync(int id);
    Task<IEnumerable<BenefitTypeDto>> GetBenefitTypesByTenantAsync();
    Task<BenefitTypeDto?> CreateBenefitTypeAsync(CreateBenefitTypeDto dto);
    Task<bool> UpdateBenefitTypeAsync(int id, UpdateBenefitTypeDto dto);
    Task<bool> DeleteBenefitTypeAsync(int id);
}
