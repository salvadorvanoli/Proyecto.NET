namespace Shared.DTOs.Benefits;

/// <summary>
/// Response after redeeming a benefit.
/// </summary>
public class RedeemBenefitResponse
{
    public int UsageId { get; set; }
    public int ConsumptionId { get; set; }
    public int BenefitId { get; set; }
    public int UserId { get; set; }
    public int RemainingUsageQuantity { get; set; }
    public int RemainingBenefitQuotas { get; set; }
    public DateTime RedeemedAt { get; set; }
    public bool IsNewUsage { get; set; }
    public string Message { get; set; } = string.Empty;
}
