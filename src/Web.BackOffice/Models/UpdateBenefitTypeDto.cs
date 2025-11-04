namespace Web.BackOffice.Models;

/// <summary>
/// DTO for updating an existing benefit type.
/// </summary>
public class UpdateBenefitTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
