using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.Responses;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Spaces;

public class IndexModel : PageModel
{
    private readonly ISpaceApiService _spaceApiService;
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<IndexModel> _logger;
    private const int PageSize = 10;

    public IndexModel(
        ISpaceApiService spaceApiService,
        ISpaceTypeApiService spaceTypeApiService,
        ILogger<IndexModel> logger)
    {
        _spaceApiService = spaceApiService;
        _spaceTypeApiService = spaceTypeApiService;
        _logger = logger;
    }

    public IEnumerable<SpaceDto> Spaces { get; set; } = Enumerable.Empty<SpaceDto>();
    public IEnumerable<SpaceDto> DisplayedSpaces { get; set; } = Enumerable.Empty<SpaceDto>();
    public IEnumerable<SpaceTypeResponse> SpaceTypes { get; set; } = Enumerable.Empty<SpaceTypeResponse>();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalSpaces { get; set; }

    // Búsqueda y filtros
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SpaceTypeFilter { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        try
        {
            // Cargar espacios y tipos de espacio
            var spacesTask = _spaceApiService.GetAllSpacesAsync();
            var spaceTypesTask = _spaceTypeApiService.GetAllSpaceTypesAsync();

            await Task.WhenAll(spacesTask, spaceTypesTask);

            Spaces = await spacesTask;
            SpaceTypes = await spaceTypesTask;

            // Aplicar filtros
            var filteredSpaces = Spaces.ToList();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredSpaces = filteredSpaces.Where(s =>
                    s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (SpaceTypeFilter.HasValue)
            {
                filteredSpaces = filteredSpaces.Where(s =>
                    s.SpaceTypeId == SpaceTypeFilter.Value
                ).ToList();
            }

            // Calcular paginación
            TotalSpaces = filteredSpaces.Count;
            TotalPages = (int)Math.Ceiling(TotalSpaces / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages == 0 ? 1 : TotalPages));

            // Obtener espacios de la página actual
            DisplayedSpaces = filteredSpaces
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading spaces");
            ErrorMessage = "Error al cargar los espacios. Por favor, intente nuevamente.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var deleted = await _spaceApiService.DeleteSpaceAsync(id);

            if (!deleted)
            {
                ErrorMessage = $"No se pudo eliminar el espacio con ID {id}. Puede que no exista.";
            }
            else
            {
                SuccessMessage = "Espacio eliminado correctamente.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space {SpaceId}", id);
            ErrorMessage = "Error al eliminar el espacio.";
            return RedirectToPage();
        }
    }
}

