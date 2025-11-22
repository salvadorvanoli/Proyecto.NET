namespace Shared.DTOs.Benefits;

/// <summary>
/// Response DTO for benefit with consumption history.
/// </summary>
public class BenefitWithHistoryResponse
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
    public List<ConsumptionHistoryResponse> Consumptions { get; set; } = new();

    /// <summary>
    /// Display-friendly validity period string.
    /// </summary>
    public string ValidityDisplay => IsPermanent 
        ? "Permanente" 
        : $"{StartDate} - {EndDate}";
}

/// <summary>
/// Response DTO for consumption history information.
/// </summary>
public class ConsumptionHistoryResponse
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public DateTime ConsumptionDateTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
