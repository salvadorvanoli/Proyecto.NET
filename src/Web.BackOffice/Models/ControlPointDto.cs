namespace Web.BackOffice.Models;

/// <summary>
/// DTO for control point response.
/// </summary>
public class ControlPointDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public string SpaceTypeName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int AccessRuleCount { get; set; }
    public int AccessEventCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
