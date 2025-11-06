using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.Benefits;

/// <summary>
/// Request for creating or updating a benefit.
/// </summary>
public class BenefitRequest
{
    [Required(ErrorMessage = "Debe seleccionar un tipo de beneficio.")]
    [Range(DomainConstants.NumericValidation.MinId, int.MaxValue, 
        ErrorMessage = "Debe seleccionar un tipo de beneficio válido.")]
    public int BenefitTypeId { get; set; }

    [Required(ErrorMessage = "Las cuotas son obligatorias.")]
    [Range(DomainConstants.NumericValidation.MinQuota, int.MaxValue, 
        ErrorMessage = "Las cuotas deben ser al menos {1}.")]
    public int Quotas { get; set; }

    public string? StartDate { get; set; }
    
    public string? EndDate { get; set; }
}
