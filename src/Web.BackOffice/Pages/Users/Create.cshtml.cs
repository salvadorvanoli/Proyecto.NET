using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Users;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IUserApiService _userApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IUserApiService userApiService, ILogger<CreateModel> logger)
    {
        _userApiService = userApiService;
        _logger = logger;
    }

    [BindProperty]
    public new CreateUserRequest User { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        User.DateOfBirth = DateTime.Now.AddYears(-18);
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
            var createdUser = await _userApiService.CreateUserAsync(User);

            TempData["SuccessMessage"] = $"Usuario '{createdUser.FullName}' creado exitosamente.";
            return RedirectToPage("/Users/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ErrorMessage = "Error al crear el usuario. Verifique que el email no esté en uso.";
            return Page();
        }
    }
}
