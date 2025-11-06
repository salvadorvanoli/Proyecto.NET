using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.Roles;

/// <summary>
/// Request for creating or updating a role.
/// </summary>
public class RoleRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.RoleNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.RoleNameMinLength, 
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Name { get; set; } = string.Empty;
}
