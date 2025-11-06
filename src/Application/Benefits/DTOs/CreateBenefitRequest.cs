using System.ComponentModel.DataAnnotations;

namespace Application.Benefits.DTOs;

/// <summary>
/// Request DTO for creating a benefit.
/// </summary>
public class CreateBenefitRequest
{
    [Required(ErrorMessage = "Debe seleccionar un tipo de beneficio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de beneficio válido.")]
    public int BenefitTypeId { get; set; }

    [Required(ErrorMessage = "Las cuotas son obligatorias.")]
    [Range(1, int.MaxValue, ErrorMessage = "Las cuotas deben ser al menos 1.")]
    public int Quotas { get; set; }

    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
