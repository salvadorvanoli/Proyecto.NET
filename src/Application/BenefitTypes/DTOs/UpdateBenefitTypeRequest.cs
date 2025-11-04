using System.ComponentModel.DataAnnotations;

namespace Application.BenefitTypes.DTOs;

/// <summary>
/// Request DTO for updating a benefit type.
/// </summary>
public class UpdateBenefitTypeRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string Description { get; set; } = string.Empty;
}
