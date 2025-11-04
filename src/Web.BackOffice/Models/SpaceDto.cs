namespace Web.BackOffice.Models;

/// <summary>
/// DTO for space response.
/// </summary>
public class SpaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SpaceTypeId { get; set; }
    public string SpaceTypeName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public int ControlPointCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
