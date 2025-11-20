using Shared.DTOs.Benefits;

namespace Application.Benefits;

/// <summary>
/// Service interface for benefit management.
/// </summary>
public interface IBenefitService
{
    Task<BenefitResponse?> GetBenefitByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BenefitResponse>> GetBenefitsByTenantAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BenefitResponse>> GetBenefitsByTypeAsync(int benefitTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BenefitResponse>> GetActiveBenefitsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableBenefitResponse>> GetAvailableBenefitsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RedeemableBenefitResponse>> GetRedeemableBenefitsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<BenefitResponse> CreateBenefitAsync(BenefitRequest request, CancellationToken cancellationToken = default);
    Task<BenefitResponse> UpdateBenefitAsync(int id, BenefitRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBenefitAsync(int id, CancellationToken cancellationToken = default);
    Task<ClaimBenefitResponse> ClaimBenefitAsync(ClaimBenefitRequest request, CancellationToken cancellationToken = default);
    Task<RedeemBenefitResponse> RedeemBenefitAsync(RedeemBenefitRequest request, CancellationToken cancellationToken = default);
}
