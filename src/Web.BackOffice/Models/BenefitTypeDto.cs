namespace Web.BackOffice.Models;

/// <summary>
/// DTO for benefit type response.
/// </summary>
public class BenefitTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int BenefitCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
