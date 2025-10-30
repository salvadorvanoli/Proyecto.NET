namespace Application.Roles.DTOs;

/// <summary>
/// Request DTO for creating a new role.
/// </summary>
public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
}

