using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.SpaceTypes;

/// <summary>
/// Request for creating or updating a space type.
/// </summary>
public class SpaceTypeRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.SpaceTypeNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.SpaceTypeNameMinLength, 
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Name { get; set; } = string.Empty;
}
