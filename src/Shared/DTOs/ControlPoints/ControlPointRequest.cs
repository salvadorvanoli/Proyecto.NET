using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.ControlPoints;

/// <summary>
/// Request for creating or updating a control point.
/// </summary>
public class ControlPointRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.ControlPointNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.ControlPointNameMinLength, 
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El espacio es obligatorio.")]
    [Range(DomainConstants.NumericValidation.MinId, int.MaxValue, 
        ErrorMessage = "Debe seleccionar un espacio válido.")]
    public int SpaceId { get; set; }
}
