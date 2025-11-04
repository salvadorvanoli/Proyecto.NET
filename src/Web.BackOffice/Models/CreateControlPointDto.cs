namespace Web.BackOffice.Models;

/// <summary>
/// DTO for creating a new control point.
/// </summary>
public class CreateControlPointDto
{
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
}
