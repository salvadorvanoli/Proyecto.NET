namespace Application.ControlPoints.DTOs;

/// <summary>
/// Request DTO for updating an existing control point.
/// </summary>
public class UpdateControlPointRequest
{
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
}
