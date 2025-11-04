using System.ComponentModel.DataAnnotations;

namespace Web.BackOffice.Models;

/// <summary>
/// DTO for benefit response.
/// </summary>
public class BenefitDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BenefitTypeId { get; set; }
    public string BenefitTypeName { get; set; } = string.Empty;
    public int Quotas { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsValid { get; set; }
    public bool HasAvailableQuotas { get; set; }
    public bool CanBeConsumed { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Display-friendly validity period string.
    /// </summary>
    public string ValidityDisplay => IsPermanent 
        ? "Permanente" 
        : $"{StartDate} - {EndDate}";

    /// <summary>
    /// Badge class based on status.
    /// </summary>
    public string StatusBadgeClass => CanBeConsumed ? "bg-success" : 
                                     IsValid ? "bg-warning" : "bg-secondary";

    /// <summary>
    /// Status text for display.
    /// </summary>
    public string StatusText => CanBeConsumed ? "Disponible" : 
                               IsValid ? "Sin cupos" : "Vencido";
}

/// <summary>
/// DTO for creating a benefit.
/// </summary>
public class CreateBenefitDto
{
    [Required(ErrorMessage = "El tipo de beneficio es obligatorio")]
    [Display(Name = "Tipo de Beneficio")]
    public int BenefitTypeId { get; set; }

    [Required(ErrorMessage = "Los cupos son obligatorios")]
    [Range(1, int.MaxValue, ErrorMessage = "Los cupos deben ser al menos 1")]
    [Display(Name = "Cupos")]
    public int Quotas { get; set; }

    [Display(Name = "Fecha de Inicio")]
    [DataType(DataType.Date)]
    public string? StartDate { get; set; }

    [Display(Name = "Fecha de Fin")]
    [DataType(DataType.Date)]
    public string? EndDate { get; set; }

    [Display(Name = "Permanente")]
    public bool IsPermanent { get; set; }
}

/// <summary>
/// DTO for updating a benefit.
/// </summary>
public class UpdateBenefitDto
{
    [Required(ErrorMessage = "El tipo de beneficio es obligatorio")]
    [Display(Name = "Tipo de Beneficio")]
    public int BenefitTypeId { get; set; }

    [Required(ErrorMessage = "Los cupos son obligatorios")]
    [Range(1, int.MaxValue, ErrorMessage = "Los cupos deben ser al menos 1")]
    [Display(Name = "Cupos")]
    public int Quotas { get; set; }

    [Display(Name = "Fecha de Inicio")]
    [DataType(DataType.Date)]
    public string? StartDate { get; set; }

    [Display(Name = "Fecha de Fin")]
    [DataType(DataType.Date)]
    public string? EndDate { get; set; }

    [Display(Name = "Permanente")]
    public bool IsPermanent { get; set; }
}
