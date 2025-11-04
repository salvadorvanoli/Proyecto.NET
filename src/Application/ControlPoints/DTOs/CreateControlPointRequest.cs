namespace Application.ControlPoints.DTOs;

/// <summary>
/// Request DTO for creating a new control point.
/// </summary>
public class CreateControlPointRequest
{
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
}
