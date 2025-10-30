namespace Application.Roles.DTOs;

/// <summary>
/// Request DTO for assigning roles to a user.
/// </summary>
public class AssignRoleRequest
{
    public List<int> RoleIds { get; set; } = new();
}

