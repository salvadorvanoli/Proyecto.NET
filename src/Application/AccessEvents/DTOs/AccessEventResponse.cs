namespace Application.AccessEvents.DTOs;

/// <summary>
/// Response DTO for access event information.
/// </summary>
public class AccessEventResponse
{
    public int Id { get; set; }
    public DateTime EventDateTime { get; set; }
    public string Result { get; set; } = string.Empty;
    public ControlPointResponse ControlPoint { get; set; } = null!;
    public int UserId { get; set; }
}

/// <summary>
/// Response DTO for control point information.
/// </summary>
public class ControlPointResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SpaceResponse Space { get; set; } = null!;
}

/// <summary>
/// Response DTO for space information.
/// </summary>
public class SpaceResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
