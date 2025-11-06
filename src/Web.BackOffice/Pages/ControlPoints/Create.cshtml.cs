using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.DTOs.ControlPoints;
using Shared.DTOs.Spaces;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class CreateModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ISpaceApiService _spaceApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IControlPointApiService controlPointApiService,
        ISpaceApiService spaceApiService,
        ILogger<CreateModel> logger)
    {
        _controlPointApiService = controlPointApiService;
        _spaceApiService = spaceApiService;
        _logger = logger;
    }

    [BindProperty]
    public ControlPointRequest ControlPoint { get; set; } = new();

    public SelectList Spaces { get; set; } = new(new List<SpaceResponse>(), "Id", "Name");

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadSpacesAsync();

            if (!Spaces.Any())
            {
                ErrorMessage = "No hay espacios disponibles. Por favor, cree al menos un espacio antes de crear un punto de control.";
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading spaces for control point creation");
            ErrorMessage = "Error al cargar los espacios.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            try
            {
                await LoadSpacesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading spaces");
                ErrorMessage = "Error al cargar los espacios.";
            }
            return Page();
        }

        try
        {
            var result = await _controlPointApiService.CreateControlPointAsync(ControlPoint);

            if (result == null)
            {
                ErrorMessage = "No se pudo crear el punto de control. Verifique que el nombre no esté duplicado en el espacio seleccionado.";
                await LoadSpacesAsync();
                return Page();
            }

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating control point");
            ErrorMessage = "Error al crear el punto de control.";
            
            try
            {
                await LoadSpacesAsync();
            }
            catch (Exception loadEx)
            {
                _logger.LogError(loadEx, "Error loading spaces after creation failure");
            }

            return Page();
        }
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
