using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Responses;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Spaces;

public class EditModel : PageModel
{
    private readonly ISpaceApiService _spaceApiService;
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        ISpaceApiService spaceApiService,
        ISpaceTypeApiService spaceTypeApiService,
        ILogger<EditModel> logger)
    {
        _spaceApiService = spaceApiService;
        _spaceTypeApiService = spaceTypeApiService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IEnumerable<SpaceTypeResponse> SpaceTypes { get; set; } = Enumerable.Empty<SpaceTypeResponse>();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del espacio es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un tipo de espacio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de espacio válido")]
        [Display(Name = "Tipo de Espacio")]
        public int SpaceTypeId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var spaceTask = _spaceApiService.GetSpaceByIdAsync(id);
            var spaceTypesTask = _spaceTypeApiService.GetAllSpaceTypesAsync();

            await Task.WhenAll(spaceTask, spaceTypesTask);

            var space = await spaceTask;
            SpaceTypes = await spaceTypesTask;

            if (space == null)
            {
                ErrorMessage = $"Espacio con ID {id} no encontrado.";
                return Page();
            }

            Input = new InputModel
            {
                Id = space.Id,
                Name = space.Name,
                SpaceTypeId = space.SpaceTypeId
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space {SpaceId}", id);
            ErrorMessage = "Error al cargar el espacio para editar.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload space types if validation fails
            try
            {
                SpaceTypes = await _spaceTypeApiService.GetAllSpaceTypesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading space types");
                ErrorMessage = "Error al cargar los tipos de espacio.";
            }
            return Page();
        }

        try
        {
            var updateSpaceDto = new UpdateSpaceDto
            {
                Name = Input.Name,
                SpaceTypeId = Input.SpaceTypeId
            };

            await _spaceApiService.UpdateSpaceAsync(Input.Id, updateSpaceDto);

            return RedirectToPage("/Spaces/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating space {SpaceId}", Input.Id);
            ErrorMessage = "Error al actualizar el espacio.";
            
            // Reload space types
            try
            {
                SpaceTypes = await _spaceTypeApiService.GetAllSpaceTypesAsync();
            }
            catch (Exception loadEx)
            {
                _logger.LogError(loadEx, "Error loading space types after update failure");
            }

            return Page();
        }
    }
}
