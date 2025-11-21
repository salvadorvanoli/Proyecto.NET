using Mobile.Models;

namespace Mobile.Services;

public interface IBenefitService
{
    Task<List<RedeemableBenefitDto>> GetRedeemableBenefitsAsync(int userId);
    Task<RedeemBenefitResponseDto> RedeemBenefitAsync(int userId, int benefitId);
}
