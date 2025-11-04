using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.AccessRules;

public class DetailsModel : PageModel
{
    private readonly IAccessRuleApiService _accessRuleApiService;

    public DetailsModel(IAccessRuleApiService accessRuleApiService)
    {
        _accessRuleApiService = accessRuleApiService;
    }

    public AccessRuleDto AccessRule { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var accessRule = await _accessRuleApiService.GetAccessRuleByIdAsync(Id);

        if (accessRule == null)
        {
            return NotFound();
        }

        AccessRule = accessRule;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var result = await _accessRuleApiService.DeleteAccessRuleAsync(Id);

        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar la regla de acceso.";
            return RedirectToPage(new { id = Id });
        }

        TempData["SuccessMessage"] = "Regla de acceso eliminada exitosamente.";
        return RedirectToPage("Index");
    }
}
