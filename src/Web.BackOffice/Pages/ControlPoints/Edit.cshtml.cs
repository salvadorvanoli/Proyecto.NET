using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.DTOs.ControlPoints;
using Shared.DTOs.Spaces;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.ControlPoints;

public class EditModel : PageModel
{
    private readonly IControlPointApiService _controlPointApiService;
    private readonly ISpaceApiService _spaceApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IControlPointApiService controlPointApiService,
        ISpaceApiService spaceApiService,
        ILogger<EditModel> logger)
    {
        _controlPointApiService = controlPointApiService;
        _spaceApiService = spaceApiService;
        _logger = logger;
    }

    [BindProperty]
    public ControlPointRequest ControlPoint { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public SelectList Spaces { get; set; } = new(new List<SpaceResponse>(), "Id", "Name");

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var controlPointTask = _controlPointApiService.GetControlPointByIdAsync(Id);
            var spacesTask = _spaceApiService.GetSpacesByTenantAsync();

            await Task.WhenAll(controlPointTask, spacesTask);

            var controlPoint = await controlPointTask;
            var spaces = await spacesTask;

            if (controlPoint == null)
            {
                ErrorMessage = $"Punto de control con ID {Id} no encontrado.";
                return Page();
            }

            ControlPoint = new ControlPointRequest
            {
                Name = controlPoint.Name,
                SpaceId = controlPoint.SpaceId
            };

            Spaces = new SelectList(
                spaces.Select(s => new { s.Id, DisplayName = $"{s.Name} ({s.SpaceTypeName})" }),
                "Id",
                "DisplayName",
                ControlPoint.SpaceId
            );

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading control point {ControlPointId}", Id);
            ErrorMessage = "Error al cargar el punto de control para editar.";
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
            var result = await _controlPointApiService.UpdateControlPointAsync(Id, ControlPoint);

            if (!result)
            {
                ErrorMessage = "No se pudo actualizar el punto de control.";
                await LoadSpacesAsync();
                return Page();
            }

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating control point {ControlPointId}", Id);
            ErrorMessage = "Error al actualizar el punto de control.";
            
            try
            {
                await LoadSpacesAsync();
            }
            catch (Exception loadEx)
            {
                _logger.LogError(loadEx, "Error loading spaces after update failure");
            }

            return Page();
        }
    }

    private async Task LoadSpacesAsync()
    {
        var spaces = await _spaceApiService.GetSpacesByTenantAsync();
        Spaces = new SelectList(
            spaces.Select(s => new { s.Id, DisplayName = $"{s.Name} ({s.SpaceTypeName})" }),
            "Id",
            "DisplayName",
            ControlPoint.SpaceId
        );
    }
}
