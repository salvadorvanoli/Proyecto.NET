using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Roles;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Roles;

public class CreateModel : PageModel
{
    private readonly IRoleApiService _roleApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IRoleApiService roleApiService, ILogger<CreateModel> logger)
    {
        _roleApiService = roleApiService;
        _logger = logger;
    }

    [BindProperty]
    public RoleRequest Role { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var createdRole = await _roleApiService.CreateRoleAsync(Role);

            TempData["SuccessMessage"] = $"Rol '{createdRole.Name}' creado exitosamente.";
            return RedirectToPage("/Roles/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            ErrorMessage = "Error al crear el rol. Verifique que el nombre no esté en uso.";
            return Page();
        }
    }
}

