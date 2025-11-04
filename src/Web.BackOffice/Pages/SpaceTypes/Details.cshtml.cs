using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.SpaceTypes;

public class DetailsModel : PageModel
{
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ISpaceTypeApiService spaceTypeApiService, ILogger<DetailsModel> logger)
    {
        _spaceTypeApiService = spaceTypeApiService;
        _logger = logger;
    }

    public SpaceTypeDto? SpaceType { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            SpaceType = await _spaceTypeApiService.GetSpaceTypeByIdAsync(id);

            if (SpaceType == null)
            {
                TempData["ErrorMessage"] = $"No se encontró el tipo de espacio con ID {id}.";
                return RedirectToPage("/SpaceTypes/Index");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space type {SpaceTypeId}", id);
            TempData["ErrorMessage"] = "Error al cargar el tipo de espacio.";
            return RedirectToPage("/SpaceTypes/Index");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var deleted = await _spaceTypeApiService.DeleteSpaceTypeAsync(id);

            if (!deleted)
            {
                TempData["ErrorMessage"] = $"No se pudo eliminar el tipo de espacio con ID {id}. Puede que no exista.";
            }
            else
            {
                TempData["SuccessMessage"] = "Tipo de espacio eliminado correctamente.";
            }

            return RedirectToPage("/SpaceTypes/Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete space type {SpaceTypeId}", id);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/SpaceTypes/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space type {SpaceTypeId}", id);
            TempData["ErrorMessage"] = "Error al eliminar el tipo de espacio.";
            return RedirectToPage("/SpaceTypes/Index");
        }
    }
}
