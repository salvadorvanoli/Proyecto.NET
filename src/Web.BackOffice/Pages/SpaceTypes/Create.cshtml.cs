using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.SpaceTypes;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.SpaceTypes;

public class CreateModel : PageModel
{
    private readonly ISpaceTypeApiService _spaceTypeApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ISpaceTypeApiService spaceTypeApiService, ILogger<CreateModel> logger)
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
        [Required(ErrorMessage = "El nombre del tipo de espacio es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new SpaceTypeRequest
            {
                Name = Input.Name
            };

            var createdSpaceType = await _spaceTypeApiService.CreateSpaceTypeAsync(request);

            TempData["SuccessMessage"] = $"Tipo de espacio '{createdSpaceType.Name}' creado exitosamente.";
            return RedirectToPage("/SpaceTypes/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating space type");
            ErrorMessage = "Error al crear el tipo de espacio. Verifique que el nombre no esté en uso.";
            return Page();
        }
    }
}
