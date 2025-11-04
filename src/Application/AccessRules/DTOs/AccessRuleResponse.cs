namespace Application.AccessRules.DTOs;

/// <summary>
/// Response DTO for access rule.
/// </summary>
public class AccessRuleResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    
    // Time range (optional - null means 24/7)
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    
    // Date range (optional - null means permanent)
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Related entities info
    public List<int> RoleIds { get; set; } = new();
    public List<string> RoleNames { get; set; } = new();
    public List<int> ControlPointIds { get; set; } = new();
    public List<string> ControlPointNames { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Computed properties
    public bool IsActive { get; set; }
    public bool Is24x7 { get; set; }
    public bool IsPermanent { get; set; }
}
