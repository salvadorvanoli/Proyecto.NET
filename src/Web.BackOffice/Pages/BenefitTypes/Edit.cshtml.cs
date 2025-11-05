using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class EditModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IBenefitTypeApiService benefitTypeApiService, ILogger<EditModel> logger)
    {
        _benefitTypeApiService = benefitTypeApiService;
        _logger = logger;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public UpdateBenefitTypeDto BenefitType { get; set; } = new();
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var benefitType = await _benefitTypeApiService.GetBenefitTypeByIdAsync(id);
            
            if (benefitType == null)
            {
                _logger.LogWarning("Benefit type with ID {Id} not found", id);
                ErrorMessage = "Tipo de beneficio no encontrado.";
                return Page();
            }

            Id = benefitType.Id;
            BenefitType = new UpdateBenefitTypeDto
            {
                Name = benefitType.Name,
                Description = benefitType.Description
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benefit type with ID {Id}", id);
            ErrorMessage = "Ocurrió un error al cargar el tipo de beneficio.";
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
            var result = await _benefitTypeApiService.UpdateBenefitTypeAsync(Id, BenefitType);
            
            if (!result)
            {
                _logger.LogWarning("Failed to update benefit type with ID {Id}", Id);
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el tipo de beneficio.");
                return Page();
            }

            _logger.LogInformation("Benefit type with ID {Id} updated successfully", Id);
            TempData["SuccessMessage"] = "Tipo de beneficio actualizado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating benefit type with ID {Id}", Id);
            ModelState.AddModelError(string.Empty, $"Error al actualizar el tipo de beneficio: {ex.Message}");
            return Page();
        }
    }
}
