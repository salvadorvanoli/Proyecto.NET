namespace Application.SpaceTypes.DTOs;

/// <summary>
/// Request DTO for creating a new space type.
/// </summary>
public class CreateSpaceTypeRequest
{
    public string Name { get; set; } = string.Empty;
}
