using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.News;
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
    public NewsRequest News { get; set; } = new();

    public int NewsId { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

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

            NewsId = newsDto.Id;
            News = new NewsRequest
            {
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        NewsId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var updatedNews = await _newsApiService.UpdateNewsAsync(id, News);

            TempData["SuccessMessage"] = $"Noticia '{updatedNews.Title}' actualizada exitosamente.";
            return RedirectToPage("/News/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news with ID {NewsId}", id);
            ErrorMessage = "Error al actualizar la noticia. Por favor, intente nuevamente.";
            return Page();
        }
    }
}

