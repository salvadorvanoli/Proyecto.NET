using System.ComponentModel.DataAnnotations;

namespace Application.AccessRules.DTOs;

/// <summary>
/// Request DTO for updating an access rule.
/// </summary>
public class UpdateAccessRuleRequest
{
    /// <summary>
    /// Start time for the access rule (HH:mm format). Null for 24/7 access.
    /// </summary>
    public string? StartTime { get; set; }
    
    /// <summary>
    /// End time for the access rule (HH:mm format). Null for 24/7 access.
    /// </summary>
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
    [Required]
    [MinLength(1, ErrorMessage = "At least one role must be assigned to the access rule.")]
    public List<int> RoleIds { get; set; } = new();
    
    /// <summary>
    /// List of control point IDs where this rule applies.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one control point must be assigned to the access rule.")]
    public List<int> ControlPointIds { get; set; } = new();
}
