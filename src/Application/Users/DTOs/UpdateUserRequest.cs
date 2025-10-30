namespace Application.Users.DTOs;

/// <summary>
/// Request DTO for updating an existing user.
/// </summary>
public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

