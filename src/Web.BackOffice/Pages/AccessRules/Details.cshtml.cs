using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.AccessRules;

public class DetailsModel : PageModel
{
    private readonly IAccessRuleApiService _accessRuleApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IAccessRuleApiService accessRuleApiService, ILogger<DetailsModel> logger)
    {
        _accessRuleApiService = accessRuleApiService;
        _logger = logger;
    }

    public AccessRuleDto AccessRule { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var accessRule = await _accessRuleApiService.GetAccessRuleByIdAsync(Id);

            if (accessRule == null)
            {
                _logger.LogWarning("Access rule with ID {Id} not found", Id);
                ErrorMessage = "La regla de acceso no fue encontrada.";
                return Page();
            }

            AccessRule = accessRule;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading access rule with ID {Id}", Id);
            ErrorMessage = "Ocurrió un error al cargar la regla de acceso.";
            return Page();
        }
    }
}
