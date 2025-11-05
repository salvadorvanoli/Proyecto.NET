using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.SpaceTypes;

/// <summary>
/// Request for creating or updating a space type.
/// </summary>
public class SpaceTypeRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;
}
