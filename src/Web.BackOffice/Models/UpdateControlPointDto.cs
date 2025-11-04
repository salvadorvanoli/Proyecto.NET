namespace Web.BackOffice.Models;

/// <summary>
/// DTO for updating an existing control point.
/// </summary>
public class UpdateControlPointDto
{
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
}
