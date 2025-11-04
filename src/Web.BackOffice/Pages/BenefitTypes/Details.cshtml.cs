using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class DetailsModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;

    public DetailsModel(IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitTypeApiService = benefitTypeApiService;
    }

    public BenefitTypeDto BenefitType { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var benefitType = await _benefitTypeApiService.GetBenefitTypeByIdAsync(id);
        
        if (benefitType == null)
        {
            TempData["ErrorMessage"] = "Tipo de beneficio no encontrado.";
            return RedirectToPage("./Index");
        }

        BenefitType = benefitType;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _benefitTypeApiService.DeleteBenefitTypeAsync(id);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el tipo de beneficio. Verifique que no tenga beneficios asociados.";
            return RedirectToPage("./Details", new { id });
        }

        TempData["SuccessMessage"] = "Tipo de beneficio eliminado exitosamente.";
        return RedirectToPage("./Index");
    }
}
