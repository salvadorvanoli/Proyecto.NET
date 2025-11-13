using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.News;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.News;

public class IndexModel : PageModel
{
    private readonly INewsApiService _newsApiService;
    private readonly ILogger<IndexModel> _logger;
    private const int PageSize = 10;

    public IndexModel(INewsApiService newsApiService, ILogger<IndexModel> logger)
    {
        _newsApiService = newsApiService;
        _logger = logger;
    }

    public IEnumerable<NewsResponse> NewsList { get; set; } = Enumerable.Empty<NewsResponse>();
    public IEnumerable<NewsResponse> DisplayedNews { get; set; } = Enumerable.Empty<NewsResponse>();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalNews { get; set; }

    // Búsqueda
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        try
        {
            NewsList = await _newsApiService.GetNewsByTenantAsync();

            // Aplicar búsqueda si hay término de búsqueda
            var filteredNews = NewsList.ToList();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredNews = filteredNews.Where(n =>
                    n.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    n.Content.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            TotalNews = filteredNews.Count;

            // Calcular paginación
            TotalPages = (int)Math.Ceiling(TotalNews / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages > 0 ? TotalPages : 1));

            // Obtener noticias para la página actual
            DisplayedNews = filteredNews
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news");
            ErrorMessage = "Error al cargar las noticias. Por favor, intente nuevamente.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var result = await _newsApiService.DeleteNewsAsync(id);

            if (result)
            {
                SuccessMessage = "Noticia eliminada exitosamente.";
            }
            else
            {
                ErrorMessage = "No se pudo encontrar la noticia a eliminar.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news with ID {NewsId}", id);
            ErrorMessage = "Error al eliminar la noticia. Por favor, intente nuevamente.";
        }

        return RedirectToPage();
    }
}
