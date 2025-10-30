using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.BackOffice.Models;
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
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;
    }

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

            Input = new InputModel
            {
                Id = role.Id,
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Verificar que no se intente editar un rol protegido
            var existingRole = await _roleApiService.GetRoleByIdAsync(Input.Id);
            if (existingRole != null && DomainConstants.SystemRoles.IsProtectedRole(existingRole.Name))
            {
                ErrorMessage = $"El rol '{existingRole.Name}' es un rol del sistema y no puede ser editado.";
                return Page();
            }

            var updateRoleDto = new UpdateRoleDto
            {
                Name = Input.Name
            };

            await _roleApiService.UpdateRoleAsync(Input.Id, updateRoleDto);

            TempData["SuccessMessage"] = $"Rol '{Input.Name}' actualizado exitosamente.";
            return RedirectToPage("/Roles/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", Input.Id);
            ErrorMessage = "Error al actualizar el rol. Verifique que el nombre no esté en uso.";
            return Page();
        }
    }
}
