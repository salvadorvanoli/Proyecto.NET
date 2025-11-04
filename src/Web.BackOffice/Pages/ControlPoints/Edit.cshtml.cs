using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class EditModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ISpaceApiService _spaceApiService;

    public EditModel(IControlPointApiService controlPointApiService, ISpaceApiService spaceApiService)
    {
        _controlPointApiService = controlPointApiService;
        _spaceApiService = spaceApiService;
    }

    [BindProperty]
    public UpdateControlPointDto ControlPoint { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public SelectList Spaces { get; set; } = new(new List<SpaceDto>(), "Id", "Name");

    public async Task<IActionResult> OnGetAsync()
    {
        var controlPoint = await _controlPointApiService.GetControlPointByIdAsync(Id);

        if (controlPoint == null)
        {
            return NotFound();
        }

        ControlPoint = new UpdateControlPointDto
        {
            Name = controlPoint.Name,
            SpaceId = controlPoint.SpaceId
        };

        await LoadSpacesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSpacesAsync();
            return Page();
        }

        var result = await _controlPointApiService.UpdateControlPointAsync(Id, ControlPoint);

        if (!result)
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el punto de control. Verifique que el nombre no esté duplicado en el espacio seleccionado.");
            await LoadSpacesAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Punto de control actualizado exitosamente.";
        return RedirectToPage("Index");
    }

    private async Task LoadSpacesAsync()
    {
        var spaces = await _spaceApiService.GetAllSpacesAsync();
        Spaces = new SelectList(
            spaces.Select(s => new { s.Id, DisplayName = $"{s.Name} ({s.SpaceTypeName})" }),
            "Id",
            "DisplayName",
            ControlPoint.SpaceId
        );
    }
}
