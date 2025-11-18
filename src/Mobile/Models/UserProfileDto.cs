namespace Mobile.Models;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime? BirthDate { get; set; }
    public bool IsActive { get; set; } = true; // Por defecto true - si el usuario existe, está activo
    public DateTime CreatedAt { get; set; }
}
