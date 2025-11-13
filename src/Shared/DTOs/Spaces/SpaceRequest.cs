using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.Spaces;

/// <summary>
/// Request for creating or updating a space.
/// </summary>
public class SpaceRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.SpaceNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.SpaceNameMinLength, 
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de espacio es obligatorio.")]
    [Range(DomainConstants.NumericValidation.MinId, int.MaxValue, 
        ErrorMessage = "Debe seleccionar un tipo de espacio válido.")]
    public int SpaceTypeId { get; set; }
}
