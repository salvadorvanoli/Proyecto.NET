using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.Users;

/// <summary>
/// Request for creating a new user.
/// </summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(DomainConstants.StringLengths.EmailMaxLength, 
        MinimumLength = DomainConstants.StringLengths.EmailMinLength, 
        ErrorMessage = "El correo electrónico debe tener entre {2} y {1} caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(DomainConstants.StringLengths.PasswordMaxLength, 
        MinimumLength = DomainConstants.StringLengths.PasswordMinLength, 
        ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.FirstNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.FirstNameMinLength, 
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.LastNameMaxLength, 
        MinimumLength = DomainConstants.StringLengths.LastNameMinLength, 
        ErrorMessage = "El apellido debe tener entre {2} y {1} caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime DateOfBirth { get; set; }
}
