namespace Web.BackOffice.Models;

/// <summary>
/// DTO for updating an existing space.
/// </summary>
public class UpdateSpaceDto
{
    public string Name { get; set; } = string.Empty;
    public int SpaceTypeId { get; set; }
}
