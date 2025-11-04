using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.AccessRules;

public class IndexModel : PageModel
{
    private readonly IAccessRuleApiService _accessRuleApiService;
    private readonly IRoleApiService _roleApiService;
    private readonly IControlPointApiService _controlPointApiService;

    public IndexModel(
        IAccessRuleApiService accessRuleApiService,
        IRoleApiService roleApiService,
        IControlPointApiService controlPointApiService)
    {
        _accessRuleApiService = accessRuleApiService;
        _roleApiService = roleApiService;
        _controlPointApiService = controlPointApiService;
    }

    public IEnumerable<AccessRuleDto> AccessRules { get; set; } = new List<AccessRuleDto>();
    public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
    public IEnumerable<ControlPointDto> ControlPoints { get; set; } = new List<ControlPointDto>();
    
    public int? SelectedRoleId { get; set; }
    public int? SelectedControlPointId { get; set; }
    public string? ActiveFilter { get; set; }

    public async Task OnGetAsync(int? roleId, int? controlPointId, string? active)
    {
        SelectedRoleId = roleId;
        SelectedControlPointId = controlPointId;
        ActiveFilter = active;

        // Load all data in parallel
        var accessRulesTask = _accessRuleApiService.GetAccessRulesByTenantAsync();
        var rolesTask = _roleApiService.GetAllRolesAsync();
        var controlPointsTask = _controlPointApiService.GetControlPointsByTenantAsync();

        await Task.WhenAll(accessRulesTask, rolesTask, controlPointsTask);

        var accessRules = await accessRulesTask;
        Roles = await rolesTask;
        ControlPoints = await controlPointsTask;

        // Apply filters
        if (SelectedRoleId.HasValue)
        {
            accessRules = accessRules.Where(ar => ar.RoleIds.Contains(SelectedRoleId.Value));
        }

        if (SelectedControlPointId.HasValue)
        {
            accessRules = accessRules.Where(ar => ar.ControlPointIds.Contains(SelectedControlPointId.Value));
        }

        if (!string.IsNullOrWhiteSpace(ActiveFilter))
        {
            if (ActiveFilter.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                accessRules = accessRules.Where(ar => ar.IsActive);
            }
            else if (ActiveFilter.Equals("inactive", StringComparison.OrdinalIgnoreCase))
            {
                accessRules = accessRules.Where(ar => !ar.IsActive);
            }
        }

        AccessRules = accessRules.OrderByDescending(ar => ar.IsActive).ThenByDescending(ar => ar.CreatedAt).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _accessRuleApiService.DeleteAccessRuleAsync(id);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar la regla de acceso.";
        }
        else
        {
            TempData["SuccessMessage"] = "Regla de acceso eliminada exitosamente.";
        }

        return RedirectToPage();
    }
}

