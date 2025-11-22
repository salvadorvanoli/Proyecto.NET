namespace Shared.DTOs;

/// <summary>
/// Access rule for offline validation
/// </summary>
public class AccessRuleDto
{
    public int UserId { get; set; }
    public int ControlPointId { get; set; }
    public int? SpaceId { get; set; }
    public string AllowedDays { get; set; } = string.Empty;
    public string StartTime { get; set; } = "00:00";
    public string EndTime { get; set; } = "23:59";
    public bool IsActive { get; set; } = true;
}
