using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class CreateModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ISpaceApiService _spaceApiService;

    public CreateModel(IControlPointApiService controlPointApiService, ISpaceApiService spaceApiService)
    {
        _controlPointApiService = controlPointApiService;
        _spaceApiService = spaceApiService;
    }

    [BindProperty]
    public CreateControlPointDto ControlPoint { get; set; } = new();

    public SelectList Spaces { get; set; } = new(new List<SpaceDto>(), "Id", "Name");

    public async Task OnGetAsync()
    {
        await LoadSpacesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSpacesAsync();
            return Page();
        }

        var result = await _controlPointApiService.CreateControlPointAsync(ControlPoint);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el punto de control. Verifique que el nombre no esté duplicado en el espacio seleccionado.");
            await LoadSpacesAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Punto de control creado exitosamente.";
        return RedirectToPage("Index");
    }

    private async Task LoadSpacesAsync()
    {
        var spaces = await _spaceApiService.GetAllSpacesAsync();
        Spaces = new SelectList(
            spaces.Select(s => new { s.Id, DisplayName = $"{s.Name} ({s.SpaceTypeName})" }),
            "Id",
            "DisplayName"
        );
    }
}
