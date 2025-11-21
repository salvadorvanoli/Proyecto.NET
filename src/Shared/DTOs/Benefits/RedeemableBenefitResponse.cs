namespace Shared.DTOs.Benefits;

/// <summary>
/// Response DTO for benefits that can be redeemed (consumed) by a user.
/// Shows Quantity from Usage (how many consumptions are available).
/// </summary>
public class RedeemableBenefitResponse
{
    public int BenefitId { get; set; }
    public int UsageId { get; set; }
    public int TenantId { get; set; }
    public int BenefitTypeId { get; set; }
    public string BenefitTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsValid { get; set; }
    public bool CanBeConsumed { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Display-friendly validity period string.
    /// </summary>
    public string ValidityDisplay => IsPermanent 
        ? "Permanente" 
        : $"{StartDate} - {EndDate}";
}
