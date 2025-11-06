using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Roles;
using Web.BackOffice.Services;
using Domain.Constants;

namespace Web.BackOffice.Pages.Roles;

public class EditModel : PageModel
{
    private readonly IRoleApiService _roleApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IRoleApiService roleApiService, ILogger<EditModel> logger)
    {
        _roleApiService = roleApiService;
        _logger = logger;
    }

    [BindProperty]
    public RoleRequest Role { get; set; } = new();

    public int RoleId { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var role = await _roleApiService.GetRoleByIdAsync(id);

            if (role == null)
            {
                TempData["ErrorMessage"] = $"No se encontró el rol con ID {id}.";
                return RedirectToPage("/Roles/Index");
            }

            // Prevenir edición de roles protegidos del sistema
            if (DomainConstants.SystemRoles.IsProtectedRole(role.Name))
            {
                TempData["ErrorMessage"] = $"El rol '{role.Name}' es un rol del sistema y no puede ser editado.";
                return RedirectToPage("/Roles/Index");
            }

            RoleId = role.Id;
            Role = new RoleRequest
            {
                Name = role.Name
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role {RoleId}", id);
            TempData["ErrorMessage"] = "Error al cargar el rol.";
            return RedirectToPage("/Roles/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        RoleId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Verificar que no se intente editar un rol protegido
            var existingRole = await _roleApiService.GetRoleByIdAsync(id);
            if (existingRole != null && DomainConstants.SystemRoles.IsProtectedRole(existingRole.Name))
            {
                ErrorMessage = $"El rol '{existingRole.Name}' es un rol del sistema y no puede ser editado.";
                return Page();
            }

            await _roleApiService.UpdateRoleAsync(id, Role);

            TempData["SuccessMessage"] = $"Rol '{Role.Name}' actualizado exitosamente.";
            return RedirectToPage("/Roles/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            ErrorMessage = "Error al actualizar el rol. Verifique que el nombre no esté en uso.";
            return Page();
        }
    }
}
