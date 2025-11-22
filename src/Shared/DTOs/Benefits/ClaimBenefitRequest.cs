using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.Benefits;

/// <summary>
/// Request for claiming a benefit.
/// </summary>
public class ClaimBenefitRequest
{
    [Required(ErrorMessage = "El ID del beneficio es obligatorio.")]
    [Range(DomainConstants.NumericValidation.MinId, int.MaxValue, 
        ErrorMessage = "Debe seleccionar un beneficio válido.")]
    public int BenefitId { get; set; }

    [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
    [Range(DomainConstants.NumericValidation.MinId, int.MaxValue, 
        ErrorMessage = "Debe seleccionar un usuario válido.")]
    public int UserId { get; set; }
}
