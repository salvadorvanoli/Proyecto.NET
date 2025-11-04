namespace Application.SpaceTypes.DTOs;

/// <summary>
/// Request DTO for updating an existing space type.
/// </summary>
public class UpdateSpaceTypeRequest
{
    public string Name { get; set; } = string.Empty;
}
