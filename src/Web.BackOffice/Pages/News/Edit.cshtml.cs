using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.News;

public class EditModel : PageModel
{
    private readonly INewsApiService _newsApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(INewsApiService newsApiService, ILogger<EditModel> logger)
    {
        _newsApiService = newsApiService;
        _logger = logger;
    }

    [BindProperty]
    public EditNewsInputModel News { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class EditNewsInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(5000, ErrorMessage = "El contenido no puede exceder 5000 caracteres")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de publicación es requerida")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string? ImageUrl { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var newsDto = await _newsApiService.GetNewsByIdAsync(id);

            if (newsDto == null)
            {
                ErrorMessage = $"Noticia con ID {id} no encontrada.";
                return Page();
            }

            News = new EditNewsInputModel
            {
                Id = newsDto.Id,
                Title = newsDto.Title,
                Content = newsDto.Content,
                PublishDate = newsDto.PublishDate,
                ImageUrl = newsDto.ImageUrl
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news for editing, ID {NewsId}", id);
            ErrorMessage = "Error al cargar la noticia para editar.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var updateNewsDto = new UpdateNewsDto
            {
                Title = News.Title,
                Content = News.Content,
                PublishDate = News.PublishDate,
                ImageUrl = News.ImageUrl
            };

            var updatedNews = await _newsApiService.UpdateNewsAsync(News.Id, updateNewsDto);

            TempData["SuccessMessage"] = $"Noticia '{updatedNews.Title}' actualizada exitosamente.";
            return RedirectToPage("/News/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news with ID {NewsId}", News.Id);
            ErrorMessage = "Error al actualizar la noticia. Por favor, intente nuevamente.";
            return Page();
        }
    }
}

