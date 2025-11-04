using System.ComponentModel.DataAnnotations;

namespace Application.Benefits.DTOs;

/// <summary>
/// Request DTO for creating a benefit.
/// </summary>
public class CreateBenefitRequest
{
    [Required(ErrorMessage = "Benefit type ID is required")]
    public int BenefitTypeId { get; set; }

    [Required(ErrorMessage = "Quotas is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quotas must be at least 1")]
    public int Quotas { get; set; }

    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
