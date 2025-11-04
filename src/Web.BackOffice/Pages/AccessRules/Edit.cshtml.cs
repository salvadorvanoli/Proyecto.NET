using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.AccessRules;

public class EditModel : PageModel
{
    private readonly IAccessRuleApiService _accessRuleApiService;
    private readonly IRoleApiService _roleApiService;
    private readonly IControlPointApiService _controlPointApiService;

    public EditModel(
        IAccessRuleApiService accessRuleApiService,
        IRoleApiService roleApiService,
        IControlPointApiService controlPointApiService)
    {
        _accessRuleApiService = accessRuleApiService;
        _roleApiService = roleApiService;
        _controlPointApiService = controlPointApiService;
    }

    [BindProperty]
    public UpdateAccessRuleDto AccessRule { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public MultiSelectList Roles { get; set; } = new(new List<RoleDto>(), "Id", "Name");
    public MultiSelectList ControlPoints { get; set; } = new(new List<ControlPointDto>(), "Id", "Name");

    [BindProperty]
    public bool Use24x7 { get; set; }

    [BindProperty]
    public bool UsePermanent { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var accessRule = await _accessRuleApiService.GetAccessRuleByIdAsync(Id);

        if (accessRule == null)
        {
            return NotFound();
        }

        AccessRule = new UpdateAccessRuleDto
        {
            StartTime = accessRule.StartTime,
            EndTime = accessRule.EndTime,
            StartDate = accessRule.StartDate,
            EndDate = accessRule.EndDate,
            RoleIds = accessRule.RoleIds,
            ControlPointIds = accessRule.ControlPointIds
        };

        Use24x7 = accessRule.Is24x7;
        UsePermanent = accessRule.IsPermanent;

        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Clear time values if 24x7 is checked
        if (Use24x7)
        {
            AccessRule.StartTime = null;
            AccessRule.EndTime = null;
        }

        // Clear date values if permanent is checked
        if (UsePermanent)
        {
            AccessRule.StartDate = null;
            AccessRule.EndDate = null;
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        // Validate at least one role and control point selected
        if (AccessRule.RoleIds == null || !AccessRule.RoleIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un rol.");
            await LoadSelectListsAsync();
            return Page();
        }

        if (AccessRule.ControlPointIds == null || !AccessRule.ControlPointIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un punto de control.");
            await LoadSelectListsAsync();
            return Page();
        }

        // Validate time range if not 24x7
        if (!Use24x7 && (string.IsNullOrWhiteSpace(AccessRule.StartTime) || string.IsNullOrWhiteSpace(AccessRule.EndTime)))
        {
            ModelState.AddModelError(string.Empty, "Debe especificar un rango horario o marcar acceso 24/7.");
            await LoadSelectListsAsync();
            return Page();
        }

        // Validate date range if not permanent
        if (!UsePermanent && (!AccessRule.StartDate.HasValue || !AccessRule.EndDate.HasValue))
        {
            ModelState.AddModelError(string.Empty, "Debe especificar un periodo de validez o marcar como permanente.");
            await LoadSelectListsAsync();
            return Page();
        }

        var result = await _accessRuleApiService.UpdateAccessRuleAsync(Id, AccessRule);

        if (!result)
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar la regla de acceso.");
            await LoadSelectListsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Regla de acceso actualizada exitosamente.";
        return RedirectToPage("Index");
    }

    private async Task LoadSelectListsAsync()
    {
        var rolesTask = _roleApiService.GetAllRolesAsync();
        var controlPointsTask = _controlPointApiService.GetControlPointsByTenantAsync();

        await Task.WhenAll(rolesTask, controlPointsTask);

        var roles = await rolesTask;
        var controlPoints = await controlPointsTask;

        Roles = new MultiSelectList(roles, "Id", "Name", AccessRule.RoleIds);
        ControlPoints = new MultiSelectList(
            controlPoints.Select(cp => new { cp.Id, DisplayName = $"{cp.Name} ({cp.SpaceName})" }),
            "Id",
            "DisplayName",
            AccessRule.ControlPointIds
        );
    }
}
