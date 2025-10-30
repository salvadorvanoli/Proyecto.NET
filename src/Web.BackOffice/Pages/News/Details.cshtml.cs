using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.News;

public class DetailsModel : PageModel
{
    private readonly INewsApiService _newsApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(INewsApiService newsApiService, ILogger<DetailsModel> logger)
    {
        _newsApiService = newsApiService;
        _logger = logger;
    }

    public NewsDto? News { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            News = await _newsApiService.GetNewsByIdAsync(id);

            if (News == null)
            {
                ErrorMessage = $"Noticia con ID {id} no encontrada.";
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news details, ID {NewsId}", id);
            ErrorMessage = "Error al cargar los detalles de la noticia.";
            return Page();
        }
    }
}
