using Shared.DTOs.BenefitTypes;

namespace Application.BenefitTypes;

/// <summary>
/// Service interface for benefit type management.
/// </summary>
public interface IBenefitTypeService
{
    Task<BenefitTypeResponse?> GetBenefitTypeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BenefitTypeResponse>> GetBenefitTypesByTenantAsync(CancellationToken cancellationToken = default);
    Task<BenefitTypeResponse> CreateBenefitTypeAsync(BenefitTypeRequest request, CancellationToken cancellationToken = default);
    Task<BenefitTypeResponse> UpdateBenefitTypeAsync(int id, BenefitTypeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBenefitTypeAsync(int id, CancellationToken cancellationToken = default);
}
