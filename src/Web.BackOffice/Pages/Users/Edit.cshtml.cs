using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Users;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.Users;

public class EditModel : PageModel
{
    private readonly IUserApiService _userApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IUserApiService userApiService, ILogger<EditModel> logger)
    {
        _userApiService = userApiService;
        _logger = logger;
    }

    [BindProperty]
    public new UpdateUserRequest User { get; set; } = new();

    public int UserId { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var userDto = await _userApiService.GetUserByIdAsync(id);

            if (userDto == null)
            {
                ErrorMessage = $"Usuario con ID {id} no encontrado.";
                return Page();
            }

            UserId = userDto.Id;
            User = new UpdateUserRequest
            {
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                DateOfBirth = userDto.DateOfBirth
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for editing, ID {UserId}", id);
            ErrorMessage = "Error al cargar el usuario para editar.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        UserId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _userApiService.UpdateUserAsync(id, User);

            return RedirectToPage("/Users/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            ErrorMessage = "Error al actualizar el usuario.";
            return Page();
        }
    }
}
