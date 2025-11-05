using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.Responses;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.SpaceTypes;

public class IndexModel : PageModel
{
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<IndexModel> _logger;
    private const int PageSize = 10;

    public IndexModel(ISpaceTypeApiService spaceTypeApiService, ILogger<IndexModel> logger)
    {
        _spaceTypeApiService = spaceTypeApiService;
        _logger = logger;
    }

    public IEnumerable<SpaceTypeResponse> SpaceTypes { get; set; } = Enumerable.Empty<SpaceTypeResponse>();
    public IEnumerable<SpaceTypeResponse> DisplayedSpaceTypes { get; set; } = Enumerable.Empty<SpaceTypeResponse>();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalSpaceTypes { get; set; }

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
            SpaceTypes = await _spaceTypeApiService.GetAllSpaceTypesAsync();

            // Aplicar búsqueda si hay término de búsqueda
            var filteredSpaceTypes = SpaceTypes.ToList();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredSpaceTypes = filteredSpaceTypes.Where(st =>
                    st.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Calcular paginación
            TotalSpaceTypes = filteredSpaceTypes.Count;
            TotalPages = (int)Math.Ceiling(TotalSpaceTypes / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages == 0 ? 1 : TotalPages));

            // Obtener tipos de espacio de la página actual
            DisplayedSpaceTypes = filteredSpaceTypes
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space types");
            ErrorMessage = "Error al cargar los tipos de espacio. Por favor, intente nuevamente.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var deleted = await _spaceTypeApiService.DeleteSpaceTypeAsync(id);

            if (!deleted)
            {
                ErrorMessage = $"No se pudo eliminar el tipo de espacio con ID {id}. Puede que no exista.";
            }
            else
            {
                SuccessMessage = "Tipo de espacio eliminado correctamente.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space type {SpaceTypeId}", id);
            ErrorMessage = "Error al eliminar el tipo de espacio.";
            return RedirectToPage();
        }
    }
}

