namespace Application.Spaces.DTOs;

/// <summary>
/// Request DTO for creating a new space.
/// </summary>
public class CreateSpaceRequest
{
    public string Name { get; set; } = string.Empty;
    public int SpaceTypeId { get; set; }
}
