using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Responses;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Spaces;

public class CreateModel : PageModel
{
    private readonly ISpaceApiService _spaceApiService;
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        ISpaceApiService spaceApiService,
        ISpaceTypeApiService spaceTypeApiService,
        ILogger<CreateModel> logger)
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
        [Required(ErrorMessage = "El nombre del espacio es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un tipo de espacio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de espacio válido")]
        [Display(Name = "Tipo de Espacio")]
        public int SpaceTypeId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            SpaceTypes = await _spaceTypeApiService.GetAllSpaceTypesAsync();

            if (!SpaceTypes.Any())
            {
                ErrorMessage = "No hay tipos de espacio disponibles. Por favor, cree al menos un tipo de espacio antes de crear un espacio.";
                return Page();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space types for space creation");
            ErrorMessage = "Error al cargar los tipos de espacio.";
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
            var createSpaceDto = new CreateSpaceDto
            {
                Name = Input.Name,
                SpaceTypeId = Input.SpaceTypeId
            };

            var createdSpace = await _spaceApiService.CreateSpaceAsync(createSpaceDto);

            return RedirectToPage("/Spaces/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space");
            ErrorMessage = "Error al crear el espacio. Verifique que el nombre no esté en uso y que el tipo de espacio sea válido.";
            
            // Reload space types
            try
            {
                SpaceTypes = await _spaceTypeApiService.GetAllSpaceTypesAsync();
            }
            catch (Exception loadEx)
            {
                _logger.LogError(loadEx, "Error loading space types after creation failure");
            }

            return Page();
        }
    }
}
