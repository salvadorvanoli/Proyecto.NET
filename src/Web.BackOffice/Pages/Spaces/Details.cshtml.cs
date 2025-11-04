using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Spaces;

public class DetailsModel : PageModel
{
    private readonly ISpaceApiService _spaceApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ISpaceApiService spaceApiService, ILogger<DetailsModel> logger)
    {
        _spaceApiService = spaceApiService;
        _logger = logger;
    }

    public SpaceDto? Space { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            Space = await _spaceApiService.GetSpaceByIdAsync(id);

            if (Space == null)
            {
                TempData["ErrorMessage"] = $"No se encontró el espacio con ID {id}.";
                return RedirectToPage("/Spaces/Index");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space {SpaceId}", id);
            TempData["ErrorMessage"] = "Error al cargar el espacio.";
            return RedirectToPage("/Spaces/Index");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var deleted = await _spaceApiService.DeleteSpaceAsync(id);

            if (!deleted)
            {
                TempData["ErrorMessage"] = $"No se pudo eliminar el espacio con ID {id}. Puede que no exista.";
            }
            else
            {
                TempData["SuccessMessage"] = "Espacio eliminado correctamente.";
            }

            return RedirectToPage("/Spaces/Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete space {SpaceId}", id);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/Spaces/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting space {SpaceId}", id);
            TempData["ErrorMessage"] = "Error al eliminar el espacio.";
            return RedirectToPage("/Spaces/Index");
        }
    }
}
