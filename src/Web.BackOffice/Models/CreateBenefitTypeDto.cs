namespace Web.BackOffice.Models;

/// <summary>
/// DTO for creating a new benefit type.
/// </summary>
public class CreateBenefitTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
