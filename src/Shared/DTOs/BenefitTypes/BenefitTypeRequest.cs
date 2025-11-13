using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.BenefitTypes;

/// <summary>
/// Request for creating or updating a benefit type.
/// </summary>
public class BenefitTypeRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.BenefitTypeNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.BenefitTypeNameMinLength, 
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(DomainConstants.StringLengths.DescriptionMaxLength, 
        MinimumLength = DomainConstants.StringLengths.DescriptionMinLength, 
        ErrorMessage = "La descripción debe tener entre {2} y {1} caracteres.")]
    public string Description { get; set; } = string.Empty;
}
