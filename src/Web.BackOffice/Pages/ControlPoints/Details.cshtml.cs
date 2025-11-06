using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.DTOs.ControlPoints;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class DetailsModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IControlPointApiService controlPointApiService, ILogger<DetailsModel> logger)
    {
        _controlPointApiService = controlPointApiService;
        _logger = logger;
    }

    public ControlPointResponse? ControlPoint { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            ControlPoint = await _controlPointApiService.GetControlPointByIdAsync(Id);

            if (ControlPoint == null)
            {
                ErrorMessage = $"Punto de control con ID {Id} no encontrado.";
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading control point details for ID {ControlPointId}", Id);
            ErrorMessage = "Error al cargar los detalles del punto de control.";
            return Page();
        }
    }
}
