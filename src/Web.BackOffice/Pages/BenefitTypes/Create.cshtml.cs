using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class CreateModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;

    public CreateModel(IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitTypeApiService = benefitTypeApiService;
    }

    [BindProperty]
    public CreateBenefitTypeDto BenefitType { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _benefitTypeApiService.CreateBenefitTypeAsync(BenefitType);
            TempData["SuccessMessage"] = "Tipo de beneficio creado exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al crear el tipo de beneficio: {ex.Message}");
            return Page();
        }
    }
}
