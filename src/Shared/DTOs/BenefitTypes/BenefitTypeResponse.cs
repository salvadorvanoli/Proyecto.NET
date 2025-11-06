namespace Shared.DTOs.BenefitTypes;

/// <summary>
/// Response DTO for benefit type.
/// </summary>
public class BenefitTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int BenefitCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
