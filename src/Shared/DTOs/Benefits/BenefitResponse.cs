namespace Shared.DTOs.Benefits;

/// <summary>
/// Response DTO for benefit information.
/// </summary>
public class BenefitResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BenefitTypeId { get; set; }
    public string BenefitTypeName { get; set; } = string.Empty;
    public int Quotas { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsValid { get; set; }
    public bool HasAvailableQuotas { get; set; }
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
