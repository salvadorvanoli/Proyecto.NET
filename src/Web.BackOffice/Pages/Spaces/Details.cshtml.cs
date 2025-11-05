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
                ErrorMessage = $"Espacio con ID {id} no encontrado.";
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space details for ID {SpaceId}", id);
            ErrorMessage = "Error al cargar los detalles del espacio.";
            return Page();
        }
    }
}
