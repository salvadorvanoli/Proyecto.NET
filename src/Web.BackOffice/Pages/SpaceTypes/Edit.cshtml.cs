using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.SpaceTypes;

public class EditModel : PageModel
{
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ISpaceTypeApiService spaceTypeApiService, ILogger<EditModel> logger)
    {
        _spaceTypeApiService = spaceTypeApiService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de espacio es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var spaceType = await _spaceTypeApiService.GetSpaceTypeByIdAsync(id);

            if (spaceType == null)
            {
                ErrorMessage = $"Tipo de espacio con ID {id} no encontrado.";
                return Page();
            }

            Input = new InputModel
            {
                Id = spaceType.Id,
                Name = spaceType.Name
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading space type {SpaceTypeId}", id);
            ErrorMessage = "Error al cargar el tipo de espacio para editar.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var updateSpaceTypeDto = new UpdateSpaceTypeDto
            {
                Name = Input.Name
            };

            await _spaceTypeApiService.UpdateSpaceTypeAsync(Input.Id, updateSpaceTypeDto);

            return RedirectToPage("/SpaceTypes/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating space type {SpaceTypeId}", Input.Id);
            ErrorMessage = "Error al actualizar el tipo de espacio.";
            return Page();
        }
    }
}
