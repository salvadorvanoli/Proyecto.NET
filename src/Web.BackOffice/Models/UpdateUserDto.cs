namespace Web.BackOffice.Models;

/// <summary>
/// DTO for updating a user.
/// </summary>
public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

