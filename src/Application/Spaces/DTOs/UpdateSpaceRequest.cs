namespace Application.Spaces.DTOs;

/// <summary>
/// Request DTO for updating an existing space.
/// </summary>
public class UpdateSpaceRequest
{
    public string Name { get; set; } = string.Empty;
    public int SpaceTypeId { get; set; }
}
