using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Benefits;

public class DetailsModel : PageModel
{
    private readonly IBenefitApiService _benefitApiService;

    public DetailsModel(IBenefitApiService benefitApiService)
    {
        _benefitApiService = benefitApiService;
    }

    public BenefitDto Benefit { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var benefit = await _benefitApiService.GetBenefitByIdAsync(id);
        
        if (benefit == null)
        {
            TempData["ErrorMessage"] = "Beneficio no encontrado.";
            return RedirectToPage("./Index");
        }

        Benefit = benefit;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _benefitApiService.DeleteBenefitAsync(id);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el beneficio.";
            return RedirectToPage("./Details", new { id });
        }

        TempData["SuccessMessage"] = "Beneficio eliminado exitosamente.";
        return RedirectToPage("./Index");
    }
}
