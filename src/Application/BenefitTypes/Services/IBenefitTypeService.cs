using Application.BenefitTypes.DTOs;

namespace Application.BenefitTypes.Services;

/// <summary>
/// Service interface for benefit type management.
/// </summary>
public interface IBenefitTypeService
{
    Task<BenefitTypeResponse?> GetBenefitTypeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BenefitTypeResponse>> GetBenefitTypesByTenantAsync(CancellationToken cancellationToken = default);
    Task<BenefitTypeResponse> CreateBenefitTypeAsync(CreateBenefitTypeRequest request, CancellationToken cancellationToken = default);
    Task<BenefitTypeResponse> UpdateBenefitTypeAsync(int id, UpdateBenefitTypeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBenefitTypeAsync(int id, CancellationToken cancellationToken = default);
}
