using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class EditModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;

    public EditModel(IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitTypeApiService = benefitTypeApiService;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public UpdateBenefitTypeDto BenefitType { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var benefitType = await _benefitTypeApiService.GetBenefitTypeByIdAsync(id);
        
        if (benefitType == null)
        {
            TempData["ErrorMessage"] = "Tipo de beneficio no encontrado.";
            return RedirectToPage("./Index");
        }

        Id = benefitType.Id;
        BenefitType = new UpdateBenefitTypeDto
        {
            Name = benefitType.Name,
            Description = benefitType.Description
        };

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
            var result = await _benefitTypeApiService.UpdateBenefitTypeAsync(Id, BenefitType);
            
            if (!result)
            {
                TempData["ErrorMessage"] = "No se pudo actualizar el tipo de beneficio.";
                return Page();
            }

            TempData["SuccessMessage"] = "Tipo de beneficio actualizado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar el tipo de beneficio: {ex.Message}");
            return Page();
        }
    }
}
