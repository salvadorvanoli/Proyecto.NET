namespace Web.BackOffice.Models;

/// <summary>
/// DTO for assigning roles to a user.
/// </summary>
public class AssignRoleDto
{
    public List<int> RoleIds { get; set; } = new();
}

