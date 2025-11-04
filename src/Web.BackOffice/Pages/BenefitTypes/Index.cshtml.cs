using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.BenefitTypes;

public class IndexModel : PageModel
{
    private readonly IBenefitTypeApiService _benefitTypeApiService;

    public IndexModel(IBenefitTypeApiService benefitTypeApiService)
    {
        _benefitTypeApiService = benefitTypeApiService;
    }

    public IEnumerable<BenefitTypeDto> BenefitTypes { get; set; } = new List<BenefitTypeDto>();
    public string? SearchTerm { get; set; }

    public async Task OnGetAsync(string? searchTerm)
    {
        SearchTerm = searchTerm;

        var benefitTypes = await _benefitTypeApiService.GetBenefitTypesByTenantAsync();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            benefitTypes = benefitTypes.Where(bt =>
                bt.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                bt.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        BenefitTypes = benefitTypes.OrderBy(bt => bt.Name).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _benefitTypeApiService.DeleteBenefitTypeAsync(id);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el tipo de beneficio. Verifique que no tenga beneficios asociados.";
        }
        else
        {
            TempData["SuccessMessage"] = "Tipo de beneficio eliminado exitosamente.";
        }

        return RedirectToPage();
    }
}
