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
                ErrorMessage = $"Tipo de espacio con ID {id} no encontrado.";
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space type details for ID {SpaceTypeId}", id);
            ErrorMessage = "Error al cargar los detalles del tipo de espacio.";
            return Page();
        }
    }
}
