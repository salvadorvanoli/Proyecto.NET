namespace Web.BackOffice.Models;

/// <summary>
/// DTO for creating a new space.
/// </summary>
public class CreateSpaceDto
{
    public string Name { get; set; } = string.Empty;
    public int SpaceTypeId { get; set; }
}
