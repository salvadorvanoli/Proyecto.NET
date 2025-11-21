namespace Shared.DTOs.Benefits;

/// <summary>
/// Response after claiming a benefit.
/// </summary>
public class ClaimBenefitResponse
{
    public int UsageId { get; set; }
    public int BenefitId { get; set; }
    public int UserId { get; set; }
    public int UsageQuantity { get; set; }
    public int RemainingBenefitQuotas { get; set; }
    public DateTime ClaimedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
