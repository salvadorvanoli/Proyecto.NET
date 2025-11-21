namespace Mobile.Models;

public class RedeemableBenefitDto
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
    
    // Property for UI binding - uses BenefitId internally
    public int Id => BenefitId;
}
