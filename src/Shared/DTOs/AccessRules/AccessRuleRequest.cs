using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.AccessRules;

/// <summary>
/// Request for creating or updating an access rule.
/// </summary>
public class AccessRuleRequest
{
    /// <summary>
    /// Start time for the access rule (HH:mm format). Null for 24/7 access.
    /// </summary>
    [RegularExpression(DomainConstants.RegexPatterns.Time24Hour, 
        ErrorMessage = "El formato de hora debe ser HH:mm (ejemplo: 09:00).")]
    public string? StartTime { get; set; }
    
    /// <summary>
    /// End time for the access rule (HH:mm format). Null for 24/7 access.
    /// </summary>
    [RegularExpression(DomainConstants.RegexPatterns.Time24Hour, 
        ErrorMessage = "El formato de hora debe ser HH:mm (ejemplo: 18:00).")]
    public string? EndTime { get; set; }
    
    /// <summary>
    /// Start date for validity period. Null for permanent access.
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End date for validity period. Null for permanent access.
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// List of role IDs that have access with this rule.
    /// </summary>
    [Required(ErrorMessage = "Debe asignar al menos un rol a la regla de acceso.")]
    [MinLength(DomainConstants.NumericValidation.MinRoleCount, 
        ErrorMessage = "Debe asignar al menos un rol a la regla de acceso.")]
    public List<int> RoleIds { get; set; } = new();
    
    /// <summary>
    /// List of control point IDs where this rule applies.
    /// </summary>
    [Required(ErrorMessage = "Debe asignar al menos un punto de control a la regla de acceso.")]
    [MinLength(DomainConstants.NumericValidation.MinControlPointCount, 
        ErrorMessage = "Debe asignar al menos un punto de control a la regla de acceso.")]
    public List<int> ControlPointIds { get; set; } = new();
}
