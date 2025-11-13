using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Shared.DTOs.News;

/// <summary>
/// Request for creating or updating a news article.
/// </summary>
public class NewsRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.TitleMaxLength, 
        MinimumLength = DomainConstants.StringLengths.TitleMinLength, 
        ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contenido es obligatorio.")]
    [StringLength(DomainConstants.StringLengths.ContentMaxLength, 
        MinimumLength = DomainConstants.StringLengths.ContentMinLength, 
        ErrorMessage = "El contenido debe tener entre {2} y {1} caracteres.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de publicación es obligatoria.")]
    public DateTime PublishDate { get; set; }

    [Url(ErrorMessage = "La URL de la imagen no es válida.")]
    [StringLength(DomainConstants.StringLengths.UrlMaxLength, 
        ErrorMessage = "La URL de la imagen no puede exceder los {1} caracteres.")]
    public string? ImageUrl { get; set; }
}
