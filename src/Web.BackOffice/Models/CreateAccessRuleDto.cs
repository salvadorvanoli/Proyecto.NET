namespace Web.BackOffice.Models;

/// <summary>
/// DTO for creating a new access rule.
/// </summary>
public class CreateAccessRuleDto
{
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<int> RoleIds { get; set; } = new();
    public List<int> ControlPointIds { get; set; } = new();
}
