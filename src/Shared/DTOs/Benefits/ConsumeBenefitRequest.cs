using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Benefits;

/// <summary>
/// Request DTO for consuming a benefit.
/// </summary>
public class ConsumeBenefitRequest
{
    /// <summary>
    /// The ID of the benefit to consume.
    /// </summary>
    [Required(ErrorMessage = "El ID del beneficio es requerido.")]
    public int BenefitId { get; set; }

    /// <summary>
    /// The quantity to consume (default is 1).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Quantity { get; set; } = 1;
}
