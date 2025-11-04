using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class DetailsModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;

    public DetailsModel(IControlPointApiService controlPointApiService)
    {
        _controlPointApiService = controlPointApiService;
    }

    public ControlPointDto ControlPoint { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var controlPoint = await _controlPointApiService.GetControlPointByIdAsync(Id);

        if (controlPoint == null)
        {
            return NotFound();
        }

        ControlPoint = controlPoint;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var result = await _controlPointApiService.DeleteControlPointAsync(Id);

        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el punto de control. Verifique que no tenga reglas o eventos de acceso asociados.";
            return RedirectToPage(new { id = Id });
        }

        TempData["SuccessMessage"] = "Punto de control eliminado exitosamente.";
        return RedirectToPage("Index");
    }
}
