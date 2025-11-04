using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class IndexModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ISpaceApiService _spaceApiService;

    public IndexModel(IControlPointApiService controlPointApiService, ISpaceApiService spaceApiService)
    {
        _controlPointApiService = controlPointApiService;
        _spaceApiService = spaceApiService;
    }

    public IEnumerable<ControlPointDto> ControlPoints { get; set; } = new List<ControlPointDto>();
    public IEnumerable<SpaceDto> Spaces { get; set; } = new List<SpaceDto>();
    public string? SearchTerm { get; set; }
    public int? SelectedSpaceId { get; set; }

    public async Task OnGetAsync(string? searchTerm, int? spaceId)
    {
        SearchTerm = searchTerm;
        SelectedSpaceId = spaceId;

        // Load control points and spaces in parallel
        var controlPointsTask = _controlPointApiService.GetControlPointsByTenantAsync();
        var spacesTask = _spaceApiService.GetAllSpacesAsync();

        await Task.WhenAll(controlPointsTask, spacesTask);

        var controlPoints = await controlPointsTask;
        Spaces = await spacesTask;

        // Apply filters
        if (SelectedSpaceId.HasValue)
        {
            controlPoints = controlPoints.Where(cp => cp.SpaceId == SelectedSpaceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            controlPoints = controlPoints.Where(cp => 
                cp.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                cp.SpaceName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                cp.SpaceTypeName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        ControlPoints = controlPoints.OrderBy(cp => cp.SpaceName).ThenBy(cp => cp.Name).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _controlPointApiService.DeleteControlPointAsync(id);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el punto de control. Verifique que no tenga reglas o eventos de acceso asociados.";
        }
        else
        {
            TempData["SuccessMessage"] = "Punto de control eliminado exitosamente.";
        }

        return RedirectToPage();
    }
}
