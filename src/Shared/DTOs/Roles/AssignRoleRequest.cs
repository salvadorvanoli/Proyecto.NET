using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.Roles;

/// <summary>
/// Request for assigning roles to a user.
/// </summary>
public class AssignRoleRequest
{
    [Required(ErrorMessage = "Debe seleccionar al menos un rol.")]
    [MinLength(DomainConstants.NumericValidation.MinRoleCount, 
        ErrorMessage = "Debe seleccionar al menos un rol.")]
    public List<int> RoleIds { get; set; } = new();
}
