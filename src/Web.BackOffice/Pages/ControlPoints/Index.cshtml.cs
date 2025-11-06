using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.ControlPoints;
using Shared.DTOs.Spaces;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class IndexModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ISpaceApiService _spaceApiService;
    private readonly ILogger<IndexModel> _logger;
    private const int PageSize = 10;

    public IndexModel(IControlPointApiService controlPointApiService, ISpaceApiService spaceApiService, ILogger<IndexModel> logger)
    {
        _controlPointApiService = controlPointApiService;
        _spaceApiService = spaceApiService;
        _logger = logger;
    }

    public IEnumerable<ControlPointResponse> ControlPoints { get; set; } = Enumerable.Empty<ControlPointResponse>();
    public IEnumerable<ControlPointResponse> DisplayedControlPoints { get; set; } = Enumerable.Empty<ControlPointResponse>();
    public IEnumerable<SpaceResponse> Spaces { get; set; } = Enumerable.Empty<SpaceResponse>();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalControlPoints { get; set; }

    // Filtros y búsqueda
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SelectedSpaceId { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        try
        {
            // Load control points and spaces in parallel
            var controlPointsTask = _controlPointApiService.GetControlPointsByTenantAsync();
            var spacesTask = _spaceApiService.GetAllSpacesAsync();

            await Task.WhenAll(controlPointsTask, spacesTask);

            var controlPoints = await controlPointsTask;
            Spaces = await spacesTask;

            // Apply filters
            var filteredControlPoints = controlPoints.ToList();

            if (SelectedSpaceId.HasValue)
            {
                filteredControlPoints = filteredControlPoints.Where(cp => cp.SpaceId == SelectedSpaceId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredControlPoints = filteredControlPoints.Where(cp => 
                    cp.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    cp.SpaceName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    cp.SpaceTypeName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Calcular paginación
            TotalControlPoints = filteredControlPoints.Count;
            TotalPages = (int)Math.Ceiling(TotalControlPoints / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(pageNumber, TotalPages == 0 ? 1 : TotalPages));

            // Obtener puntos de control de la página actual
            ControlPoints = filteredControlPoints.OrderBy(cp => cp.SpaceName).ThenBy(cp => cp.Name).ToList();
            DisplayedControlPoints = ControlPoints
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading control points");
            ErrorMessage = "Error al cargar los puntos de control. Por favor, intente nuevamente.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var result = await _controlPointApiService.DeleteControlPointAsync(id);
            
            if (!result)
            {
                ErrorMessage = "No se pudo eliminar el punto de control. Verifique que no tenga reglas o eventos de acceso asociados.";
            }
            else
            {
                SuccessMessage = "Punto de control eliminado exitosamente.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting control point {ControlPointId}", id);
            ErrorMessage = "Error al eliminar el punto de control. Por favor, intente nuevamente.";
            return RedirectToPage();
        }
    }
}
